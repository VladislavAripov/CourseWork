﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using YouTrackIntegration.Data;

namespace YouTrackIntegration.Data.Migrations
{
    [DbContext(typeof(MyAppContext))]
    [Migration("20210606105804_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.6");

            modelBuilder.Entity("YouTrackIntegration.Data.ClockifyYouTrackAssociation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("defaultIssueId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("domain")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("permToken")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("workspaceId")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Associations");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            defaultIssueId = "CAYTI-2",
                            domain = "https://coursework.myjetbrains.com/youtrack",
                            permToken = "perm:cm9vdA==.NDktMA==.dGM8QEEi9ToWNX7Xta2wSDFdN2xCbE",
                            workspaceId = "608ab876b2a2b737f32693d9"
                        });
                });

            modelBuilder.Entity("YouTrackIntegration.Data.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<int>("ClockifyYouTrackAssociationId")
                        .HasColumnType("int");

                    b.Property<string>("clockifyUserId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("defaultIssueId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("youTrackUserLogin")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Users");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            ClockifyYouTrackAssociationId = 1,
                            clockifyUserId = "608ab876b2a2b737f32693d8",
                            youTrackUserLogin = "pesoshin"
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
