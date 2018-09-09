using MAVAppBackend.EF;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace MAVAppBackend
{
    public class DI
    {
        public static IServiceProvider ServiceProvider { get; set; }
    }
}
