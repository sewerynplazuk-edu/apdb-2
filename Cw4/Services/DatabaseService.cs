using System;
using System.Data.SqlClient;
using Cw4.Models;

namespace Cw4.Services
{
	public class DatabaseService : IDatabaseService
	{
        private readonly IConfiguration _configuration;

        public DatabaseService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> DoesProductExist(int idProduct)
        {
            using var connection = new SqlConnection(GetConnectionString());
            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT 1 FROM Product WHERE IdProduct = @idProduct";
            command.Parameters.AddWithValue("@idProduct", idProduct);
            await connection.OpenAsync();
            try
            {
                var result = await command.ExecuteReaderAsync();
                return result.HasRows;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<Order?> GetOrder(int idProduct, int amount, DateTime createdAt)
        {
            using var connection = new SqlConnection(GetConnectionString());
            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT TOP 1 * FROM \"Order\" WHERE IdProduct = @idProduct AND Amount = @amount AND CreatedAt < @createdAt";
            command.Parameters.AddWithValue("@idProduct", idProduct);
            command.Parameters.AddWithValue("@amount", amount);
            command.Parameters.AddWithValue("@createdAt", createdAt);
            await connection.OpenAsync();
            try
            {
                using var reader = await command.ExecuteReaderAsync();
                await reader.ReadAsync();
                return new Order {
                    IdOrder = Convert.ToInt32(reader["IdOrder"]),
                    IdProduct = Convert.ToInt32(reader["IdProduct"]),
                    Amount = Convert.ToInt32(reader["Amount"]),
                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                    FulfilledAt = reader["FulfilledAt"] == DBNull.Value ? null : Convert.ToDateTime(reader["FulfilledAt"])
                };
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool?> HasOrderBeenProcessed(int idOrder)
        {
            using var connection = new SqlConnection(GetConnectionString());
            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT 1 FROM Product_Warehouse WHERE IdOrder = @idOrder";
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            await connection.OpenAsync();
            try
            {
                using var reader = await command.ExecuteReaderAsync();
                return reader.HasRows;
            }
            catch
            {
                return null;
            }
        }

        public async Task<int> RegisterProduct(SomeKindOfProduct productDTO, int idOrder)
        {
            using var connection = new SqlConnection(GetConnectionString());
            using var command = connection.CreateCommand();
            await connection.OpenAsync();
            var transaction = await connection.BeginTransactionAsync();
            command.Transaction = transaction as SqlTransaction;

            var now = DateTime.Now;

            try
            {
                command.CommandText = $"UPDATE \"Order\" SET FulfilledAt = @fulfilledAt WHERE IdOrder = @idOrder";
                command.Parameters.AddWithValue("@fulfilledAt", DateTime.Now);
                command.Parameters.AddWithValue("@idOrder", idOrder);

                await command.ExecuteNonQueryAsync();
                command.Parameters.Clear();

                command.CommandText = $"SELECT Price FROM Product WHERE IdProduct = @idProduct";
                command.Parameters.AddWithValue("@idProduct", productDTO.IdProduct);

                using var reader = await command.ExecuteReaderAsync();
                await reader.ReadAsync();
                double productPrice = Convert.ToDouble(reader["Price"]);
                await reader.CloseAsync();
                command.Parameters.Clear();

                command.CommandText = $"INSERT INTO Product_Warehouse(IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt) " +
                    $"OUTPUT INSERTED.IdProductWarehouse " +
                    $"VALUES (@idWarehouse, @idProduct, @idOrder, @amount, @price, @createAt)";
                command.Parameters.AddWithValue("@idWarehouse", productDTO.IdWarehouse);
                command.Parameters.AddWithValue("@idProduct", productDTO.IdProduct);
                command.Parameters.AddWithValue("@idOrder", idOrder);
                command.Parameters.AddWithValue("@amount", productDTO.Amount);
                command.Parameters.AddWithValue("@price", productPrice * productDTO.Amount);
                command.Parameters.AddWithValue("@createAt", now);

                var result = await command.ExecuteScalarAsync();
                await transaction.CommitAsync();
                return Convert.ToInt32(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await transaction.RollbackAsync();
                return -1;
            }
        }

        public async Task<int> RunRegisterProductProcedure(SomeKindOfProduct productDTO)
        {
            using var connection = new SqlConnection(GetConnectionString());
            using var command = connection.CreateCommand();
            await connection.OpenAsync();

            command.CommandText = "AddProductToWarehouse";
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@IdProduct", productDTO.IdProduct);
            command.Parameters.AddWithValue("@IdWarehouse", productDTO.IdWarehouse);
            command.Parameters.AddWithValue("@Amount", productDTO.Amount);
            command.Parameters.AddWithValue("@CreatedAt", productDTO.CreatedAt);
            try
            {
                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            catch
            {
                return -1;
            }
        }

        private String GetConnectionString()
        {
            return _configuration.GetConnectionString("Default");
        }
    }
}

