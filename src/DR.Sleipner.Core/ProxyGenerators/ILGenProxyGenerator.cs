﻿using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace DR.Sleipner.Core.ProxyGenerators
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
            var lookupHandlerField = typeBuilder.DefineField("handler", typeof(ILookupHandler<T>), FieldAttributes.Private);
            
            //Create the constructor
            var cTorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new[] { interfaceType, typeof(ILookupHandler<T>) });
            cTorBuilder.DefineParameter(1, ParameterAttributes.None, "realInstance");
            cTorBuilder.DefineParameter(2, ParameterAttributes.None, "handler");

            //Create constructor body
            var cTorBody = cTorBuilder.GetILGenerator();

            cTorBody.Emit(OpCodes.Ldarg_0);
            cTorBody.Emit(OpCodes.Call, typeof(Object).GetConstructor(Type.EmptyTypes));

            cTorBody.Emit(OpCodes.Ldarg_0);                     //Load this on stack
            cTorBody.Emit(OpCodes.Ldarg_1);                     //Load the first parameter (the realInstance parameter) of the constructor on stack
            cTorBody.Emit(OpCodes.Stfld, realInstanceField);    //Store parameter reference in realInstanceField

            cTorBody.Emit(OpCodes.Ldarg_0);                     //Load this on stack
            cTorBody.Emit(OpCodes.Ldarg_2);                     //Load second parameter on stack (the IProxyHandler parameter).
            cTorBody.Emit(OpCodes.Stfld, lookupHandlerField);         //Store parameter refrence in handlerField

            cTorBody.Emit(OpCodes.Ret);                         //Return

            foreach (var method in interfaceType.GetMethods()) //We guarantee internally that this is the methods of an interface. The compiler will gurantee that these are all the methods that needs proxying.
            {
                var parameterTypes = method.GetParameters().Select(a => a.ParameterType).ToArray();
                var proxyMethod = typeBuilder.DefineMethod(
                    method.Name,
                    MethodAttributes.Public | MethodAttributes.Virtual,
                    CallingConventions.Standard | CallingConventions.HasThis,
                    method.ReturnType,
                    parameterTypes);
                
                if (method.IsGenericMethod)
                {
                    var genericTypes = method.GetGenericArguments();
                    proxyMethod.DefineGenericParameters(genericTypes.Select(a => a.Name).ToArray());
                }

                var methodIndex = interfaceType.GetMethods().ToList().IndexOf(method);
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

                /* Load the methodinfo of the current method into a local variable */

                var methodInfoLocal = methodBody.DeclareLocal(typeof (MethodInfo));
                methodBody.Emit(OpCodes.Ldtoken, typeof(T));                                            //typeof(T)
                methodBody.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));             //typeof(T) NOTICE USE OF CALL INSTEAD OF CALLVIRT
                methodBody.Emit(OpCodes.Callvirt, typeof(Type).GetMethod("GetMethods", new Type[0]));   //.GetMethods(new Type[0])
                methodBody.Emit(OpCodes.Ldc_I4, methodIndex);                                           //Read Array Index x
                methodBody.Emit(OpCodes.Ldelem, typeof(MethodInfo));                                    //As an methodinfo
                if (method.IsGenericMethod)
                {
                    var genericTypes = method.GetGenericArguments();
                    var genericTypesArray = methodBody.DeclareLocal(typeof (Type[]));
                    methodBody.Emit(OpCodes.Ldc_I4, genericTypes.Length);
                    methodBody.Emit(OpCodes.Newarr, typeof(Type));
                    methodBody.Emit(OpCodes.Stloc, genericTypesArray);

                    for (var i = 0; i < genericTypes.Length; i++)
                    {
                        var genericType = genericTypes[i];
                        methodBody.Emit(OpCodes.Ldloc, genericTypesArray);
                        methodBody.Emit(OpCodes.Ldc_I4, i);
                        methodBody.Emit(OpCodes.Ldtoken, genericType);
                        methodBody.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
                        methodBody.Emit(OpCodes.Stelem_Ref);
                    }

                    methodBody.Emit(OpCodes.Ldloc, genericTypesArray);
                    methodBody.Emit(OpCodes.Callvirt, typeof(MethodInfo).GetMethod("MakeGenericMethod"));
                }
                methodBody.Emit(OpCodes.Stloc, methodInfoLocal);                                    //And store it
                
                /* The below code creates an array that contains all the values
                 * that were passed into the method we're proxying.
                 */
                var methodParameterArray = methodBody.DeclareLocal(typeof(object[]));
                
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

                var proxyCallMethod = typeof(ILookupHandler<T>).GetMethod("GetResult");
                proxyCallMethod = proxyCallMethod.MakeGenericMethod(new[] {method.ReturnType});

                /* This generates a method call to the proxyMethod handler field. */
                //var cachedItem = methodBody.DeclareLocal(method.ReturnType);
                methodBody.Emit(OpCodes.Ldarg_0);
                methodBody.Emit(OpCodes.Ldfld, lookupHandlerField);                                   //Load this on the stack
                methodBody.Emit(OpCodes.Ldloc, methodInfoLocal);
                methodBody.Emit(OpCodes.Ldloc, methodParameterArray);
                methodBody.Emit(OpCodes.Callvirt, proxyCallMethod);                             //Call the interceptMethod
                //methodBody.Emit(OpCodes.Stloc, cachedItem);                                     //Store the result of the method call in a local variable. This also pops it from the stack.
                //methodBody.Emit(OpCodes.Ldloc, cachedItem);                                     //Load cached item on the stack
                methodBody.Emit(OpCodes.Ret);                                                   //Return to caller
                
                typeBuilder.DefineMethodOverride(proxyMethod, method);
            }

            var createdType = typeBuilder.CreateType();
            //AssemblyBuilder.Save("SleipnerCacheProxies.dll");
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