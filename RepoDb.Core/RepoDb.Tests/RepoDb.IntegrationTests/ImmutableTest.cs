﻿using System;
using Microsoft.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RepoDb.Extensions;
using RepoDb.IntegrationTests.Models;
using RepoDb.IntegrationTests.Setup;

namespace RepoDb.IntegrationTests
{
    [TestClass]
    public class ImmutableTest
    {
        [TestInitialize]
        public void Initialize()
        {
            Database.Initialize();
            Cleanup();
        }

        [TestCleanup]
        public void Cleanup()
        {
            Database.Cleanup();
        }

        #region Delete

        [TestMethod]
        public void TestSqlConnectionDeleteForImmutableViaDataEntity()
        {
            // Setup
            var entity = Helper.CreateImmutableIdentityTable();

            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Act
                connection.Insert<ImmutableIdentityTable>(entity);

                // The ID could not be set back to the entities, so it should be 0

                // Act
                var deleteResult = connection.Delete<ImmutableIdentityTable>(entity);

                // Assert
                Assert.IsTrue(deleteResult == 0);
                Assert.AreEqual(1, connection.CountAll<ImmutableIdentityTable>());
            }
        }

        [TestMethod]
        public void TestSqlConnectionDeleteForImmutableViaPrimary()
        {
            // Setup
            var entity = Helper.CreateImmutableIdentityTable();

            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Act
                connection.Insert<ImmutableIdentityTable>(entity);

                // The ID could not be set back to the entities, so it should be 0

                // Act
                var deleteResult = connection.Delete<ImmutableIdentityTable>(entity.Id);

                // Assert
                Assert.IsTrue(deleteResult == 0);
                Assert.AreEqual(1, connection.CountAll<ImmutableIdentityTable>());
            }
        }

        #endregion

        #region Insert

        [TestMethod]
        public void TestSqlConnectionInsertForImmutable()
        {
            // Setup
            var entity = Helper.CreateImmutableIdentityTable();

            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Act
                var insertResult = connection.Insert<ImmutableIdentityTable, long>(entity);

                // Assert
                Assert.IsTrue(insertResult > 0);

                // The ID could not be set back to the entity, so it should be 0

                // Assert
                Assert.AreEqual(0, entity.Id);
            }
        }

        #endregion

        #region InsertAll

        [TestMethod]
        public void TestSqlConnectionInsertAllForImmutable()
        {
            // Setup
            var entities = Helper.CreateImmutableIdentityTables(10);

            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Act
                var insertAllResult = connection.InsertAll<ImmutableIdentityTable>(entities);

                // Assert
                Assert.AreEqual(entities.Count, insertAllResult);
                Assert.AreEqual(entities.Count, connection.CountAll<ImmutableIdentityTable>());

                // The ID could not be set back to the entities, so it should be 0

                // Assert
                Assert.IsTrue(entities.All(e => e.Id == 0));
            }
        }

        #endregion

        #region Merge

        [TestMethod]
        public void TestSqlConnectionMergeForImmutable()
        {
            // Setup
            var entity = Helper.CreateImmutableIdentityTable();

            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Act
                var mergeResult = connection.Merge<ImmutableIdentityTable, long>(entity);

                // Assert
                Assert.IsTrue(mergeResult > 0);
                Assert.AreEqual(1, connection.CountAll<ImmutableIdentityTable>());

                // The ID could not be set back to the entities, so it should be 0

                // Assert
                Assert.IsTrue(entity.Id == 0);

                // Act
                var queryResult = connection.Query<ImmutableIdentityTable>(mergeResult).FirstOrDefault();

                // Assert
                Helper.AssertPropertiesEquality(entity, queryResult);
            }
        }

        [TestMethod]
        public void TestSqlConnectionMergeForImmutableWithNonEmptyTable()
        {
            // Setup
            var entity = Helper.CreateImmutableIdentityTable();

            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Act
                var insertResult = connection.Merge<ImmutableIdentityTable, long>(entity);

                // The ID could not be set back to the entities, so it should be 0

                // Setup
                var newEntity = new ImmutableIdentityTable(insertResult,
                    entity.RowGuid,
                    false,
                    entity.ColumnDateTime,
                    DateTime.UtcNow,
                    entity.ColumnDecimal,
                    entity.ColumnFloat,
                    entity.ColumnInt,
                    entity.ColumnNVarChar);

                // Act
                var mergeResult = connection.Merge<ImmutableIdentityTable, long>(newEntity);

                // Assert
                Assert.IsTrue(mergeResult > 0);
                Assert.AreEqual(entity.Id, mergeResult);

                // Act
                var queryResult = connection.Query<ImmutableIdentityTable>(newEntity.Id).FirstOrDefault();

                // Assert
                Helper.AssertPropertiesEquality(newEntity, queryResult);
            }
        }

        #endregion

        #region MergeAll

        [TestMethod]
        public void TestSqlConnectionMergeAllForImmutable()
        {
            // Setup
            var entities = Helper.CreateImmutableIdentityTables(10);

            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Act
                var mergeAllRequest = connection.MergeAll<ImmutableIdentityTable>(entities);

                // Assert
                Assert.AreEqual(entities.Count, mergeAllRequest);

                // The ID could not be set back to the entities, so it should be 0

                // Assert
                Assert.IsTrue(entities.All(e => e.Id == 0));

                // Act
                var queryResult = connection.QueryAll<ImmutableIdentityTable>().AsList();

                // Assert
                Assert.AreEqual(entities.Count, queryResult.Count());
                entities.ForEach(entity =>
                    Helper.AssertPropertiesEquality(entity, queryResult[entities.IndexOf(entity)]));
            }
        }

        [TestMethod]
        public void TestSqlConnectionMergeAllForImmutableWithNonEmptyTables()
        {
            // Setup
            var entities = Helper.CreateImmutableIdentityTables(10);

            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Act
                var insertAllResult = connection.InsertAll<ImmutableIdentityTable>(entities);

                // Setup
                var newEntities = entities.Select(entity => new ImmutableIdentityTable(entity.Id,
                    entity.RowGuid,
                    false,
                    entity.ColumnDateTime,
                    DateTime.UtcNow,
                    entity.ColumnDecimal,
                    entity.ColumnFloat,
                    entity.ColumnInt,
                    entity.ColumnNVarChar)).AsList();

                // Act
                var mergeAllResult = connection.MergeAll<ImmutableIdentityTable>(newEntities);

                // Assert
                Assert.AreEqual(entities.Count, mergeAllResult);

                // The ID could not be set back to the entities, so it should be 0

                // Assert
                Assert.IsTrue(entities.All(e => e.Id == 0));

                // Act
                var queryResult = connection.QueryAll<ImmutableIdentityTable>().AsList();

                // Assert
                Assert.AreEqual(entities.Count, queryResult.Count());
                newEntities.ForEach(entity =>
                    Helper.AssertPropertiesEquality(entity, queryResult[entities.IndexOf(entity)]));
            }
        }

        #endregion

        #region Query

        [TestMethod]
        public void TestSqlConnectionQueryForImmutable()
        {
            // Setup
            var entity = Helper.CreateImmutableIdentityTable();

            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Act
                var insertResult = connection.Insert<ImmutableIdentityTable, long>(entity);

                // Act
                var queryResult = connection.Query<ImmutableIdentityTable>(insertResult).FirstOrDefault();

                // Assert
                Helper.AssertPropertiesEquality(entity, queryResult);
            }
        }

        #endregion

        #region Update

        [TestMethod]
        public void TestSqlConnectionUpdateForImmutableViaDataEntity()
        {
            // Setup
            var entity = Helper.CreateImmutableIdentityTable();

            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Act
                connection.Insert<ImmutableIdentityTable, long>(entity);

                // Setup
                var newEntity = new ImmutableIdentityTable(entity.Id,
                    entity.RowGuid,
                    false,
                    entity.ColumnDateTime,
                    DateTime.UtcNow,
                    entity.ColumnDecimal,
                    entity.ColumnFloat,
                    entity.ColumnInt,
                    entity.ColumnNVarChar);

                // Act
                var updateResult = connection.Update<ImmutableIdentityTable>(newEntity);

                // Assert
                Assert.IsTrue(updateResult > 0);

                // Act
                var queryResult = connection.Query<ImmutableIdentityTable>(newEntity.Id).FirstOrDefault();

                // Assert
                Assert.IsNotNull(queryResult);
                Helper.AssertPropertiesEquality(newEntity, queryResult);
            }
        }

        [TestMethod]
        public void TestSqlConnectionUpdateForImmutableViaPrimaryKey()
        {
            // Setup
            var entity = Helper.CreateImmutableIdentityTable();

            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Act
                connection.Insert<ImmutableIdentityTable, long>(entity);

                // Setup
                var newEntity = new ImmutableIdentityTable(entity.Id,
                    entity.RowGuid,
                    false,
                    entity.ColumnDateTime,
                    DateTime.UtcNow,
                    entity.ColumnDecimal,
                    entity.ColumnFloat,
                    entity.ColumnInt,
                    entity.ColumnNVarChar);

                // Act
                var updateResult = connection.Update<ImmutableIdentityTable>(newEntity, newEntity.Id);

                // Assert
                Assert.IsTrue(updateResult > 0);

                // Act
                var queryResult = connection.Query<ImmutableIdentityTable>(newEntity.Id).FirstOrDefault();

                // Assert
                Assert.IsNotNull(queryResult);
                Helper.AssertPropertiesEquality(newEntity, queryResult);
            }
        }

        #endregion

        #region UpdateAll

        [TestMethod]
        public void TestSqlConnectionUpdateAllForImmutable()
        {
            // Setup
            var entities = Helper.CreateImmutableIdentityTables(10);

            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Act
                connection.InsertAll<ImmutableIdentityTable>(entities);

                // Setup
                var newEntities = entities.Select(entity => new ImmutableIdentityTable(entity.Id,
                    entity.RowGuid,
                    false,
                    entity.ColumnDateTime,
                    DateTime.UtcNow,
                    entity.ColumnDecimal,
                    entity.ColumnFloat,
                    entity.ColumnInt,
                    entity.ColumnNVarChar)).AsList();

                // Act
                var updateAllResult = connection.UpdateAll<ImmutableIdentityTable>(newEntities);

                // Assert
                Assert.AreEqual(entities.Count, updateAllResult);

                // Act
                var queryResult = connection.QueryAll<ImmutableIdentityTable>().AsList();

                // Assert
                Assert.AreEqual(entities.Count, queryResult.Count());
                newEntities.ForEach(entity =>
                    Helper.AssertPropertiesEquality(entity, queryResult[entities.IndexOf(entity)]));
            }
        }

        #endregion
    }
}
