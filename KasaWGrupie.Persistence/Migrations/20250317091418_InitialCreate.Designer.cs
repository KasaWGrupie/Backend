﻿// <auto-generated />
using KasaWGrupie.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KasaWGrupie.Persistence.Migrations
{
    [DbContext(typeof(KasaWGrupieDbContext))]
    [Migration("20250317091418_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("GroupPayRequest", b =>
                {
                    b.Property<int>("GroupsToSettleId")
                        .HasColumnType("integer");

                    b.Property<int>("PayRequestId")
                        .HasColumnType("integer");

                    b.HasKey("GroupsToSettleId", "PayRequestId");

                    b.HasIndex("PayRequestId");

                    b.ToTable("GroupPayRequest");
                });

            modelBuilder.Entity("GroupUser", b =>
                {
                    b.Property<int>("GroupsId")
                        .HasColumnType("integer");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("GroupsId", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("UserGroups", (string)null);
                });

            modelBuilder.Entity("GroupUser1", b =>
                {
                    b.Property<int>("AdministratedGroupsId")
                        .HasColumnType("integer");

                    b.Property<int>("User1Id")
                        .HasColumnType("integer");

                    b.HasKey("AdministratedGroupsId", "User1Id");

                    b.HasIndex("User1Id");

                    b.ToTable("UserAdministratedGroups", (string)null);
                });

            modelBuilder.Entity("GroupUser2", b =>
                {
                    b.Property<int>("GroupId")
                        .HasColumnType("integer");

                    b.Property<int>("MembersId")
                        .HasColumnType("integer");

                    b.HasKey("GroupId", "MembersId");

                    b.HasIndex("MembersId");

                    b.ToTable("GroupUser2");
                });

            modelBuilder.Entity("KasaWGrupie.Core.Entities.Currency", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("Id");

                    b.ToTable("Currencies");
                });

            modelBuilder.Entity("KasaWGrupie.Core.Entities.Expense", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("ExpenseSplitId")
                        .HasColumnType("integer");

                    b.Property<int>("GroupId")
                        .HasColumnType("integer");

                    b.Property<int>("PayingPersonId")
                        .HasColumnType("integer");

                    b.Property<string>("PictureUrl")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.HasKey("Id");

                    b.HasIndex("GroupId");

                    b.HasIndex("PayingPersonId");

                    b.ToTable("Expenses");
                });

            modelBuilder.Entity("KasaWGrupie.Core.Entities.ExpenseSplit", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("ExpenseId")
                        .HasColumnType("integer");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ExpenseId")
                        .IsUnique();

                    b.ToTable("ExpenseSplits");
                });

            modelBuilder.Entity("KasaWGrupie.Core.Entities.ExpenseSplitRecord", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("ExpenseSplitId")
                        .HasColumnType("integer");

                    b.Property<int>("OwingPersonId")
                        .HasColumnType("integer");

                    b.Property<decimal>("Percentage")
                        .HasColumnType("decimal(3,2)");

                    b.HasKey("Id");

                    b.HasIndex("ExpenseSplitId");

                    b.HasIndex("OwingPersonId");

                    b.ToTable("ExpenseSplitRecords");
                });

            modelBuilder.Entity("KasaWGrupie.Core.Entities.FriendRequest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("ReceiverId")
                        .HasColumnType("integer");

                    b.Property<int>("SenderId")
                        .HasColumnType("integer");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ReceiverId");

                    b.HasIndex("SenderId");

                    b.ToTable("FriendRequests");
                });

            modelBuilder.Entity("KasaWGrupie.Core.Entities.Group", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("AdminId")
                        .HasColumnType("integer");

                    b.Property<int>("CurrencyId")
                        .HasColumnType("integer");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("PictureUrl")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("AdminId");

                    b.HasIndex("CurrencyId");

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("KasaWGrupie.Core.Entities.JoinRequest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("GroupId")
                        .HasColumnType("integer");

                    b.Property<int>("RequestingUserId")
                        .HasColumnType("integer");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("GroupId");

                    b.HasIndex("RequestingUserId");

                    b.ToTable("JoinRequests");
                });

            modelBuilder.Entity("KasaWGrupie.Core.Entities.MoneyTransfer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("GroupId")
                        .HasColumnType("integer");

                    b.Property<int>("RecipientId")
                        .HasColumnType("integer");

                    b.Property<int>("SenderId")
                        .HasColumnType("integer");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("GroupId");

                    b.HasIndex("RecipientId");

                    b.HasIndex("SenderId");

                    b.ToTable("MoneyTransfers");
                });

            modelBuilder.Entity("KasaWGrupie.Core.Entities.PayRequest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("ReceiverId")
                        .HasColumnType("integer");

                    b.Property<int>("SenderId")
                        .HasColumnType("integer");

                    b.Property<int>("payRequstStatus")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ReceiverId");

                    b.HasIndex("SenderId");

                    b.ToTable("PayRequests");
                });

            modelBuilder.Entity("KasaWGrupie.Core.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("ProfilePictureUrl")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("UserUser", b =>
                {
                    b.Property<int>("FriendsId")
                        .HasColumnType("integer");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("FriendsId", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("UserFriends", (string)null);
                });

            modelBuilder.Entity("GroupPayRequest", b =>
                {
                    b.HasOne("KasaWGrupie.Core.Entities.Group", null)
                        .WithMany()
                        .HasForeignKey("GroupsToSettleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("KasaWGrupie.Core.Entities.PayRequest", null)
                        .WithMany()
                        .HasForeignKey("PayRequestId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("GroupUser", b =>
                {
                    b.HasOne("KasaWGrupie.Core.Entities.Group", null)
                        .WithMany()
                        .HasForeignKey("GroupsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("KasaWGrupie.Core.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("GroupUser1", b =>
                {
                    b.HasOne("KasaWGrupie.Core.Entities.Group", null)
                        .WithMany()
                        .HasForeignKey("AdministratedGroupsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("KasaWGrupie.Core.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("User1Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("GroupUser2", b =>
                {
                    b.HasOne("KasaWGrupie.Core.Entities.Group", null)
                        .WithMany()
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("KasaWGrupie.Core.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("MembersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("KasaWGrupie.Core.Entities.Expense", b =>
                {
                    b.HasOne("KasaWGrupie.Core.Entities.Group", "Group")
                        .WithMany("Expenses")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("KasaWGrupie.Core.Entities.User", "PayingPerson")
                        .WithMany()
                        .HasForeignKey("PayingPersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");

                    b.Navigation("PayingPerson");
                });

            modelBuilder.Entity("KasaWGrupie.Core.Entities.ExpenseSplit", b =>
                {
                    b.HasOne("KasaWGrupie.Core.Entities.Expense", "Expense")
                        .WithOne("ExpenseSplit")
                        .HasForeignKey("KasaWGrupie.Core.Entities.ExpenseSplit", "ExpenseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Expense");
                });

            modelBuilder.Entity("KasaWGrupie.Core.Entities.ExpenseSplitRecord", b =>
                {
                    b.HasOne("KasaWGrupie.Core.Entities.ExpenseSplit", "ExpenseSplit")
                        .WithMany("SplitRecords")
                        .HasForeignKey("ExpenseSplitId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("KasaWGrupie.Core.Entities.User", "OwingPerson")
                        .WithMany()
                        .HasForeignKey("OwingPersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ExpenseSplit");

                    b.Navigation("OwingPerson");
                });

            modelBuilder.Entity("KasaWGrupie.Core.Entities.FriendRequest", b =>
                {
                    b.HasOne("KasaWGrupie.Core.Entities.User", "Receiver")
                        .WithMany("RecievedFriendRequests")
                        .HasForeignKey("ReceiverId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("KasaWGrupie.Core.Entities.User", "Sender")
                        .WithMany("SentFriendRequests")
                        .HasForeignKey("SenderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Receiver");

                    b.Navigation("Sender");
                });

            modelBuilder.Entity("KasaWGrupie.Core.Entities.Group", b =>
                {
                    b.HasOne("KasaWGrupie.Core.Entities.User", "Admin")
                        .WithMany()
                        .HasForeignKey("AdminId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("KasaWGrupie.Core.Entities.Currency", "Currency")
                        .WithMany()
                        .HasForeignKey("CurrencyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Admin");

                    b.Navigation("Currency");
                });

            modelBuilder.Entity("KasaWGrupie.Core.Entities.JoinRequest", b =>
                {
                    b.HasOne("KasaWGrupie.Core.Entities.Group", "Group")
                        .WithMany("JoinRequests")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("KasaWGrupie.Core.Entities.User", "RequestingUser")
                        .WithMany()
                        .HasForeignKey("RequestingUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");

                    b.Navigation("RequestingUser");
                });

            modelBuilder.Entity("KasaWGrupie.Core.Entities.MoneyTransfer", b =>
                {
                    b.HasOne("KasaWGrupie.Core.Entities.Group", "Group")
                        .WithMany("MoneyTransfers")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("KasaWGrupie.Core.Entities.User", "Recipient")
                        .WithMany()
                        .HasForeignKey("RecipientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("KasaWGrupie.Core.Entities.User", "Sender")
                        .WithMany()
                        .HasForeignKey("SenderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");

                    b.Navigation("Recipient");

                    b.Navigation("Sender");
                });

            modelBuilder.Entity("KasaWGrupie.Core.Entities.PayRequest", b =>
                {
                    b.HasOne("KasaWGrupie.Core.Entities.User", "Receiver")
                        .WithMany("RecievedPayRequests")
                        .HasForeignKey("ReceiverId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("KasaWGrupie.Core.Entities.User", "Sender")
                        .WithMany("SentPayRequests")
                        .HasForeignKey("SenderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Receiver");

                    b.Navigation("Sender");
                });

            modelBuilder.Entity("UserUser", b =>
                {
                    b.HasOne("KasaWGrupie.Core.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("FriendsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("KasaWGrupie.Core.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("KasaWGrupie.Core.Entities.Expense", b =>
                {
                    b.Navigation("ExpenseSplit")
                        .IsRequired();
                });

            modelBuilder.Entity("KasaWGrupie.Core.Entities.ExpenseSplit", b =>
                {
                    b.Navigation("SplitRecords");
                });

            modelBuilder.Entity("KasaWGrupie.Core.Entities.Group", b =>
                {
                    b.Navigation("Expenses");

                    b.Navigation("JoinRequests");

                    b.Navigation("MoneyTransfers");
                });

            modelBuilder.Entity("KasaWGrupie.Core.Entities.User", b =>
                {
                    b.Navigation("RecievedFriendRequests");

                    b.Navigation("RecievedPayRequests");

                    b.Navigation("SentFriendRequests");

                    b.Navigation("SentPayRequests");
                });
#pragma warning restore 612, 618
        }
    }
}
