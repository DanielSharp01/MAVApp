﻿// <auto-generated />
using System;
using MAVAppBackend.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MAVAppBackend.Migrations
{
    [DbContext(typeof(MAVAppContext))]
    partial class MAVAppContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.1-rtm-30846")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("MAVAppBackend.EF.Station", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.Property<string>("NormName")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.ToTable("Stations");
                });

            modelBuilder.Entity("MAVAppBackend.EF.Trace", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("DateTime");

                    b.Property<int>("DelayMinutes");

                    b.Property<int>("TrainInstanceId");

                    b.HasKey("Id");

                    b.HasIndex("TrainInstanceId");

                    b.ToTable("Trace");
                });

            modelBuilder.Entity("MAVAppBackend.EF.Train", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("EncodedPolyline");

                    b.Property<DateTime?>("ExpiryDate");

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<int>("Number");

                    b.Property<string>("Type")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.ToTable("Trains");
                });

            modelBuilder.Entity("MAVAppBackend.EF.TrainInstance", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ElviraId")
                        .HasMaxLength(16);

                    b.Property<int>("TrainId");

                    b.HasKey("Id");

                    b.HasIndex("TrainId");

                    b.ToTable("TrainInstances");
                });

            modelBuilder.Entity("MAVAppBackend.EF.TrainInstanceStation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<TimeSpan?>("ActualArrival");

                    b.Property<TimeSpan?>("ActualDeparture");

                    b.Property<int>("TrainInstanceId");

                    b.Property<int>("TrainStationId");

                    b.HasKey("Id");

                    b.HasIndex("TrainInstanceId");

                    b.HasIndex("TrainStationId");

                    b.ToTable("TrainInstanceStations");
                });

            modelBuilder.Entity("MAVAppBackend.EF.TrainStation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<TimeSpan?>("Arrival");

                    b.Property<TimeSpan?>("Departure");

                    b.Property<int>("Ordinal");

                    b.Property<string>("Platform")
                        .HasMaxLength(16);

                    b.Property<double?>("RelativeDistance");

                    b.Property<int>("StationId");

                    b.Property<int>("TrainId");

                    b.HasKey("Id");

                    b.HasIndex("StationId");

                    b.HasIndex("TrainId");

                    b.ToTable("TrainStations");
                });

            modelBuilder.Entity("MAVAppBackend.EF.Station", b =>
                {
                    b.OwnsOne("MAVAppBackend.EF.DbVector2", "GPSCoord", b1 =>
                        {
                            b1.Property<int>("StationId");

                            b1.Property<double?>("X")
                                .HasColumnName("Latitude");

                            b1.Property<double?>("Y")
                                .HasColumnName("Longitude");

                            b1.ToTable("Stations");

                            b1.HasOne("MAVAppBackend.EF.Station")
                                .WithOne("GPSCoord")
                                .HasForeignKey("MAVAppBackend.EF.DbVector2", "StationId")
                                .OnDelete(DeleteBehavior.Cascade);
                        });
                });

            modelBuilder.Entity("MAVAppBackend.EF.Trace", b =>
                {
                    b.HasOne("MAVAppBackend.EF.TrainInstance", "TrainInstance")
                        .WithMany("Traces")
                        .HasForeignKey("TrainInstanceId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.OwnsOne("MAVAppBackend.EF.DbVector2", "GPSCoord", b1 =>
                        {
                            b1.Property<int>("TraceId");

                            b1.Property<double?>("X")
                                .HasColumnName("Latitude");

                            b1.Property<double?>("Y")
                                .HasColumnName("Longitude");

                            b1.ToTable("Trace");

                            b1.HasOne("MAVAppBackend.EF.Trace")
                                .WithOne("GPSCoord")
                                .HasForeignKey("MAVAppBackend.EF.DbVector2", "TraceId")
                                .OnDelete(DeleteBehavior.Cascade);
                        });
                });

            modelBuilder.Entity("MAVAppBackend.EF.TrainInstance", b =>
                {
                    b.HasOne("MAVAppBackend.EF.Train", "Train")
                        .WithMany()
                        .HasForeignKey("TrainId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MAVAppBackend.EF.TrainInstanceStation", b =>
                {
                    b.HasOne("MAVAppBackend.EF.TrainInstance", "TrainInstance")
                        .WithMany("TrainInstanceStations")
                        .HasForeignKey("TrainInstanceId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("MAVAppBackend.EF.TrainStation", "TrainStation")
                        .WithMany()
                        .HasForeignKey("TrainStationId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MAVAppBackend.EF.TrainStation", b =>
                {
                    b.HasOne("MAVAppBackend.EF.Station", "Station")
                        .WithMany()
                        .HasForeignKey("StationId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("MAVAppBackend.EF.Train", "Train")
                        .WithMany("TrainStations")
                        .HasForeignKey("TrainId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
