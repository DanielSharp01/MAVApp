using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend.EF
{
    public class MAVAppContext : DbContext
    {
        public MAVAppContext(DbContextOptions<MAVAppContext> options)
            : base(options)
        { }

        public DbSet<Station> Stations { get; set; }
        public DbSet<Train> Trains { get; set; }
        public DbSet<TrainStation> TrainStations { get; set; }
        public DbSet<TrainInstance> TrainInstances { get; set; }
        public DbSet<TrainInstanceStation> TrainInstanceStations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Station>().Property(p => p.Name);
            var stationGpsCoord = modelBuilder.Entity<Station>().OwnsOne(p => p.GPSCoord);
            stationGpsCoord.Property(p => p.X).HasColumnName("Latitude");
            stationGpsCoord.Property(p => p.Y).HasColumnName("Longitude");

            modelBuilder.Entity<Train>().Property(t => t.EncodedPolyline).UsePropertyAccessMode(PropertyAccessMode.Property);
            modelBuilder.Entity<Train>().HasMany(c => c.TrainStations).WithOne(p => p.Train).IsRequired();
            modelBuilder.Entity<TrainInstance>().HasMany(c => c.TrainInstanceStations).WithOne(p => p.TrainInstance).IsRequired();
            modelBuilder.Entity<TrainInstance>().HasMany(c => c.Traces).WithOne(p => p.TrainInstance).IsRequired();

            var traceGpsCoord = modelBuilder.Entity<Trace>().OwnsOne(p => p.GPSCoord);
            traceGpsCoord.Property(p => p.X).HasColumnName("Latitude");
            traceGpsCoord.Property(p => p.Y).HasColumnName("Longitude");
        }
    }
}
