# Sleipner
This is a caching, and general performance enhancement, library for the .NET Framework version 4 and above. Sleipner is also Odin's Eight Legged Horse.

![My little Sleipner](http://24.media.tumblr.com/tumblr_m4jjr17oGK1rwcxwko1_500.png)

## About Sleipner
Sleipner is desgined to be an automatic wrapper around repositories and services to ensure high availablility and performance by backing the results of method invocations in memcached. There is also a Dictionary-backed CacheProvider available which can be used during development, or in environments that cannot use Memcached. However using Memcached is always recommended, as this ensures high availability.

## Using Sleipner
By default Sleipner comes with an Enyim Memcached-based Memcached Provider (the EnyimMemcachedProvider<T>-class). You can use the typical way of configuring EnyimMemcached:

```xml
<configSections>
  <sectionGroup name="enyim.com">
    <section name="memcached" type="Enyim.Caching.Configuration.MemcachedClientSection, Enyim.Caching" />
  </sectionGroup>
</configSections>
<enyim.com>
  <memcached protocol="Binary">
    <servers>
      <add address="localhost" port="11211" />
    </servers>
    <transcoder type="DR.Sleipner.EnyimMemcachedProvider.Transcoders.NewtonsoftTranscoder, DR.Sleipner.EnyimMemcachedProvider" factory="DR.Sleipner.EnyimMemcachedProvider.Transcoders.NewtonsoftProvider, DR.Sleipner.EnyimMemcachedProvider" />
  </memcached>
</enyim.com>

```

You then create a SleipnerProxy object that wraps around your Repository or Service class. Your Repo/Service must have an interface that specifies it's communication contract.

```csharp
var myAwesomeProxy = new SleipnerProxy<IAwesomeService>(
    new AwesomeService(),
    new EnyimMemcachedProvider<IAwesomeService>(_memcachedClientInstance)
);

myAwesomeProxy.Configure(a => {
    a.ForAll().CacheFor(120); //By default everything is cached in memcached for 2 minutes
    a.For(b => b.AwesomeMethod("", 0).CacheFor(10); //Any result from the AwesomeMethod is only cached for 10 seconds though
});

var myAwesomeService = myAwesomeProxy.Object; //the myAwesomeService object is now backed in memcached.

```
## Need to know
Remember to adhere to the recommended usage pattern for MemcachedClient. It is a heavy object and should be kept in a static context. While the SleipnerProxy-class is not heavy to initialize, there is no reason not to keep it in a static context as well. Most IoC Frameworks (such as StructureMap and Ninject) supports doing this by configuration. Alternatively you can keep the instances as static members of your IOC Initialization class or in your Application-file (in web apps).