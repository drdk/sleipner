﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using DR.Sleipner.CacheProviders;

namespace DR.Sleipner.CacheProxy.Generators
{
    public class ILGenProxyGenerator : IProxyGenerator
    {
        private static readonly AssemblyBuilder AssemblyBuilder;
        private static readonly ModuleBuilder ModuleBuilder;

        static ILGenProxyGenerator()
        {
            var currentDomain = AppDomain.CurrentDomain;
            var dynamicAssemblyName = new AssemblyName
                              {
                                  Name = "SleipnerCacheProxies",
                              };

            AssemblyBuilder = currentDomain.DefineDynamicAssembly(dynamicAssemblyName, AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder = AssemblyBuilder.DefineDynamicModule("SleipnerCacheProxies", "SleipnerCacheProxies.dll");
        }

        public Type CreateProxy<T>() where T : class
        {
            var interfaceType = typeof(T);
            var typeBuilder = getTypebuilder<T>(interfaceType.FullName + "__Proxy");

            //Create two class members: handler and realInstance
            var realInstanceField = typeBuilder.DefineField("realInstance", interfaceType, FieldAttributes.Private);
            var handlerField = typeBuilder.DefineField("handler", typeof (IProxyHandler<T>), FieldAttributes.Private);
            
            //Create the constructor
            var cTorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new[] { interfaceType, typeof(IProxyHandler<T>) });
            cTorBuilder.DefineParameter(1, ParameterAttributes.None, "realInstance");
            cTorBuilder.DefineParameter(2, ParameterAttributes.None, "handler");

            //Create constructor body
            var cTorBody = cTorBuilder.GetILGenerator();

            cTorBody.Emit(OpCodes.Ldarg_0);                     //Load this on stack
            cTorBody.Emit(OpCodes.Ldarg_1);                     //Load the first parameter (the realInstance parameter) of the constructor on stack
            cTorBody.Emit(OpCodes.Stfld, realInstanceField);    //Store parameter reference in realInstanceField

            cTorBody.Emit(OpCodes.Ldarg_0);                     //Load this on stack
            cTorBody.Emit(OpCodes.Ldarg_2);                     //Load second parameter on stack (the IProxyHandler parameter).
            cTorBody.Emit(OpCodes.Stfld, handlerField);         //Store parameter refrence in handlerField

            cTorBody.Emit(OpCodes.Ret);                         //Return

            var proxyCallMethod = typeof(IProxyHandler<T>).GetMethod("HandleRequest");
            
            foreach (var method in interfaceType.GetMethods()) //We guarantee internally that this is the methods of an interface. The compiler will gurantee that these are all the methods that needs proxying.
            {
                var parameterTypes = method.GetParameters().Select(a => a.ParameterType).ToArray();
                var proxyMethod = typeBuilder.DefineMethod(method.Name, MethodAttributes.Public | MethodAttributes.Virtual, CallingConventions.HasThis, method.ReturnType, parameterTypes);
                var methodBody = proxyMethod.GetILGenerator();
                
                /* This below code creates a pass through proxy method that does nothing. It just forwards everything to real instance.
                 * This is used for void method and method that do no define a cachebehavior */

                if (method.ReturnType == typeof(void))
                {
                    methodBody.Emit(OpCodes.Ldarg_0);                           //Load this on the stack
                    methodBody.Emit(OpCodes.Ldfld, realInstanceField);          //Load the real instance on the stack
                    for (var i = 0; i < parameterTypes.Length; i++)             //Load all parameters on the stack
                    {
                        methodBody.Emit(OpCodes.Ldarg, i + 1);                  //Method parameter on stack
                    }

                    methodBody.Emit(OpCodes.Callvirt, method);                  //Call the method in question on the instance
                    methodBody.Emit(OpCodes.Ret);                               //Return to caller.

                    continue;
                }

                /* The below code creates an array that contains all the values
                 * that were passed into the method we're proxying.
                 */
                var methodParameterArray = methodBody.DeclareLocal(typeof(object[]));
                var cachedItem = methodBody.DeclareLocal(method.ReturnType);

                methodBody.Emit(OpCodes.Ldc_I4, parameterTypes.Length);     //Push array size on stack
                methodBody.Emit(OpCodes.Newarr, typeof(object));            //Create array
                methodBody.Emit(OpCodes.Stloc, methodParameterArray);       //Store array in local variable

                for (var i = 0; i < parameterTypes.Length; i++)             //Load all parameters on the stack
                {
                    var parameterType = parameterTypes[i];
                    methodBody.Emit(OpCodes.Ldloc, methodParameterArray);   //Push array reference
                    methodBody.Emit(OpCodes.Ldc_I4, i);                     //Push array index index
                    methodBody.Emit(OpCodes.Ldarg, i + 1);                  //Push array index value

                    if (parameterType.IsValueType)
                    {
                        methodBody.Emit(OpCodes.Box, parameterType);        //Value types need to be boxed
                    }

                    methodBody.Emit(OpCodes.Stelem_Ref);                    //Store element in array
                }

                /* This generates a method call to the proxyMethod handler field. */
                methodBody.Emit(OpCodes.Ldarg_0);
                methodBody.Emit(OpCodes.Ldfld, handlerField);                                                                   //Load this on the stack
                methodBody.Emit(OpCodes.Ldstr, method.Name);                                                                    //Load the first parameter value on the stack (name of the method being called)
                methodBody.Emit(OpCodes.Ldloc, methodParameterArray);                                                           //Load the array on the stack
                methodBody.Emit(OpCodes.Callvirt, proxyCallMethod.MakeGenericMethod(new []{ method.ReturnType })); //Call the interceptMethod
                methodBody.Emit(OpCodes.Stloc, cachedItem);                                                                     //Store the result of the method call in a local variable. This also pops it from the stack.
                methodBody.Emit(OpCodes.Ldloc, cachedItem);                                                                     //Load cached item on the stack
                methodBody.Emit(OpCodes.Ret);                                                                                   //Return to caller

                typeBuilder.DefineMethodOverride(proxyMethod, method);
            }

            var createdType = typeBuilder.CreateType();
            return createdType;
        }

        private TypeBuilder getTypebuilder<T>(string name)
        {
            var existing = ModuleBuilder.GetType(name, ignoreCase: true, throwOnError: false);
            var mutator = 1;
            var mutatedname = name;
            while (existing != null)
            {
                mutatedname = name + "_" + (mutator++);
                existing = ModuleBuilder.GetType(mutatedname, ignoreCase: true, throwOnError: false);
            }
            return ModuleBuilder.DefineType(mutatedname, TypeAttributes.Class | TypeAttributes.Public, null, new [] { typeof(T) });
        }
    }
}