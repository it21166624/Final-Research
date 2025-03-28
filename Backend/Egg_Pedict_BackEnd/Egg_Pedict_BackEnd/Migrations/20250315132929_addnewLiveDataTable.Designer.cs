﻿// <auto-generated />
using System;
using Egg_Pedict_BackEnd.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Egg_Pedict_BackEnd.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20250315132929_addnewLiveDataTable")]
    partial class addnewLiveDataTable
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Egg_Pedict_BackEnd.Model.LDRData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("LDRValue")
                        .HasColumnType("integer");

                    b.Property<string>("LightStatus")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("LDRData");
                });

            modelBuilder.Entity("Egg_Pedict_BackEnd.Model.LiveData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<double>("Egg_count")
                        .HasColumnType("double precision");

                    b.Property<double>("Feed_Quantity")
                        .HasColumnType("double precision");

                    b.Property<string>("Health_Status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<double>("Hen_Age_weeks")
                        .HasColumnType("double precision");

                    b.Property<double>("Hen_Count")
                        .HasColumnType("double precision");

                    b.Property<double>("Humidity")
                        .HasColumnType("double precision");

                    b.Property<double>("Light_Hours")
                        .HasColumnType("double precision");

                    b.Property<double>("Temperature")
                        .HasColumnType("double precision");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("LiveData");
                });

            modelBuilder.Entity("Egg_Pedict_BackEnd.Model.SenserDataNew", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<double>("Egg_count")
                        .HasColumnType("double precision");

                    b.Property<double>("Feed_Quantity")
                        .HasColumnType("double precision");

                    b.Property<string>("Health_Status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<double>("Hen_Age_weeks")
                        .HasColumnType("double precision");

                    b.Property<double>("Hen_Count")
                        .HasColumnType("double precision");

                    b.Property<double>("Humidity")
                        .HasColumnType("double precision");

                    b.Property<double>("Light_Hours")
                        .HasColumnType("double precision");

                    b.Property<double>("Temperature")
                        .HasColumnType("double precision");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("senserDataNew");
                });

            modelBuilder.Entity("Egg_Pedict_BackEnd.Model.SensorData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<double>("Egg_count")
                        .HasColumnType("double precision");

                    b.Property<double>("Feed_Quantity")
                        .HasColumnType("double precision");

                    b.Property<string>("Health_Status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<double>("Hen_Age_weeks")
                        .HasColumnType("double precision");

                    b.Property<double>("Hen_Count")
                        .HasColumnType("double precision");

                    b.Property<double>("Humidity")
                        .HasColumnType("double precision");

                    b.Property<double>("Light_Hours")
                        .HasColumnType("double precision");

                    b.Property<double>("Temperature")
                        .HasColumnType("double precision");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("SensorData");
                });

            modelBuilder.Entity("Egg_Pedict_BackEnd.Model.StressData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("AirQuality")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<double>("BodyTemperature")
                        .HasColumnType("double precision");

                    b.Property<double>("CageDensity")
                        .HasColumnType("double precision");

                    b.Property<double>("FeedIntakePerHen")
                        .HasColumnType("double precision");

                    b.Property<int>("Heartbeat")
                        .HasColumnType("integer");

                    b.Property<double>("Humidity")
                        .HasColumnType("double precision");

                    b.Property<double>("Lighting")
                        .HasColumnType("double precision");

                    b.Property<int>("StressLevel")
                        .HasColumnType("integer");

                    b.Property<double>("Temperature")
                        .HasColumnType("double precision");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<double>("Vocalization")
                        .HasColumnType("double precision");

                    b.Property<double>("WaterIntakePerHen")
                        .HasColumnType("double precision");

                    b.HasKey("Id");

                    b.ToTable("StressData");
                });
#pragma warning restore 612, 618
        }
    }
}
