﻿using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecretSanta.Domain.Models;
using SecretSanta.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SecretSanta.Domain.Tests.Services
{
    [TestClass]
    public class GroupServicesTest
    {
        private SqliteConnection SqliteConnection { get; set; }
        private DbContextOptions<ApplicationDbContext> Options { get; set; }

        private Group CreateGroup(string title)
        {
            Group group = new Group
            {
                Title = "Family",
                UserGroups = new List<UserGroups>()
            };
            return group;
        }

        private User CreateUser(string f, string l)
        {
            return new User { FirstName = f, LastName = l };
        }

        [TestInitialize]
        public void OpenConnection()
        {
            SqliteConnection = new SqliteConnection("DataSource=:memory:");
            SqliteConnection.Open();

            Options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(SqliteConnection)
                .Options;

            using (var context = new ApplicationDbContext(Options))
            {
                context.Database.EnsureCreated();
            }
        }

        [TestCleanup]
        public void CloseConnection()
        {
            SqliteConnection.Close();
        }

        [TestMethod]
        public void CreateGroup_Add3Users_ToGroup()
        {
            GroupService groupService;
            Group group = CreateGroup("Family");

            User user1 = CreateUser("Conner", "Verret");
            User user2 = CreateUser("Carter", "Verret");
            User user3 = CreateUser("Paul", "Verret");

            group.UserGroups.Add(CreateUserGroup(user1, group));
            group.UserGroups.Add(CreateUserGroup(user2, group));
            group.UserGroups.Add(CreateUserGroup(user3, group));

            using (var context = new ApplicationDbContext(Options))
            {
                groupService = new GroupService(context);
                groupService.UpsertGroup(group);
            }

            using (var context = new ApplicationDbContext(Options))
            {
                groupService = new GroupService(context);
                group = groupService.Find(1);

                Assert.AreEqual("Family", group.Title);
                Assert.AreEqual("Conner", group.UserGroups[0].User.FirstName);
                Assert.AreEqual("Paul", group.UserGroups[2].User.FirstName);
            }
        }

        [TestMethod]
        public void CreateGroup_AddAndRemoveUser_FromGroup()
        {
            GroupService groupService;
            Group group = CreateGroup("Family");

            User user1 = CreateUser("Conner", "Verret");
            User user2 = CreateUser("Carter", "Verret");
            User user3 = CreateUser("Paul", "Verret");

            group.UserGroups.Add(CreateUserGroup(user1, group));
            group.UserGroups.Add(CreateUserGroup(user2, group));
            group.UserGroups.Add(CreateUserGroup(user3, group));

            using (var context = new ApplicationDbContext(Options))
            {
                groupService = new GroupService(context);
                groupService.UpsertGroup(group);
            }

            using (var context = new ApplicationDbContext(Options))
            {
                groupService = new GroupService(context);
                groupService.RemoveUserFromGroup(group.Id, group.UserGroups[0].UserId);
            }

            using (var context = new ApplicationDbContext(Options))
            {
                groupService = new GroupService(context);
                group = groupService.Find(1);

                User user = group.UserGroups[0].User;
                Assert.AreEqual("Carter", user.FirstName);
                Assert.AreEqual(2, group.UserGroups.Count);
            }
        }

        private UserGroups CreateUserGroup(User user, Group group)
        {
            UserGroups userGroups = new UserGroups
            {
                User = user,
                UserId = user.Id,
                Group = group,
                GroupId = group.Id
            };
            return userGroups;
        }
    }
}
