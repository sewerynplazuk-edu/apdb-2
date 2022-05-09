using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Cw5
{
	public class DatabaseService : IDatabaseService
	{
		private static readonly string ConnectionString = @"Server = localhost,1433; Database=Master; User Id = SA; Password=1234567890Qaz; MultipleActiveResultSets=True";

		public async Task<bool> DoesProductExist(int idProduct)
        {
            try
            {
                using var connection = new SqlConnection(ConnectionString);
                using var command = connection.CreateCommand();
                await connection.OpenAsync();
                command.CommandText = $"SELECT 1 FROM Product WHERE IdProduct = @idProduct";
                command.Parameters.AddWithValue("@idProduct", idProduct);
                var result = await command.ExecuteReaderAsync();
                return result.HasRows;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> GetOrderId(int idProduct, int amount, DateTime createdAt)
        {
            try
            {
                using var connection = new SqlConnection(ConnectionString);
                using var command = connection.CreateCommand();
                await connection.OpenAsync();
                command.CommandText = $"SELECT TOP 1 IdOrder FROM \"Order\" WHERE IdProduct = @idProduct AND Amount = @amount AND CreatedAt < @createdAt";
                command.Parameters.AddWithValue("@idProduct", idProduct);
                command.Parameters.AddWithValue("@amount", amount);
                command.Parameters.AddWithValue("@createdAt", createdAt);
                var reader = await command.ExecuteReaderAsync();
                await reader.ReadAsync();
                return int.Parse(reader["IdOrder"].ToString());
            }
            catch
            {
                return -1;
            }
        }

        public async Task<bool> HasOrderBeenProcessed(int idOrder)
        {
            try
            {
                using var connection = new SqlConnection(ConnectionString);
                using var command = connection.CreateCommand();
                await connection.OpenAsync();
                command.CommandText = $"SELECT 1 FROM Product_Warehouse WHERE IdOrder = @idOrder";
                command.Parameters.AddWithValue("@IdOrder", idOrder);
                var result = await command.ExecuteReaderAsync();
                return result.HasRows;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> RegisterProduct(ProductMetadata productMetadata, int idOrder)
        {
            using var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync();
            var transaction = await connection.BeginTransactionAsync();

            using var command = connection.CreateCommand();
            command.Transaction = transaction as SqlTransaction;

            try
            {
                command.CommandText = $"UPDATE \"Order\" SET FulfilledAt = @fulfilledAt WHERE IdProduct = @idProduct";
                command.Parameters.AddWithValue("@fulfilledAt", DateTime.Now);
                command.Parameters.AddWithValue("@idProduct", productMetadata.IdProduct);
                await command.ExecuteNonQueryAsync();

                command.CommandText = $"SELECT TOP 1 Price FROM Product WHERE IdProduct = @idProduct2";
                command.Parameters.AddWithValue("@idProduct2", productMetadata.IdProduct);
                var productPriceReader = await command.ExecuteReaderAsync();
                await productPriceReader.ReadAsync();
                var productPrice = double.Parse(productPriceReader["Price"].ToString());
                var price = productPrice * productMetadata.Amount;
                productPriceReader.Close();

                command.CommandText = $"INSERT INTO Product_Warehouse(IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt) " +
                    $"output INSERTED.idproductwarehouse " +
                    $"VALUES (@idWarehouse, @idProduct3, @idOrder, @amount, @price, @createAt)";
                command.Parameters.AddWithValue("@idWarehouse", productMetadata.IdWarehouse);
                command.Parameters.AddWithValue("@idProduct3", productMetadata.IdProduct);
                command.Parameters.AddWithValue("@idOrder", idOrder);
                command.Parameters.AddWithValue("@amount", productMetadata.Amount);
                command.Parameters.AddWithValue("@price", price);
                command.Parameters.AddWithValue("@createAt", DateTime.Now);

                var insertionResult = await command.ExecuteScalarAsync();
                var primaryKey = int.Parse(insertionResult.ToString());
                await transaction.CommitAsync();
                return primaryKey;
            }
            catch
            {
                await transaction.RollbackAsync();
                return -1;
            }
        }

        public async Task<int> RegisterProductViaProcedure(ProductMetadata productMetadata) {
            using var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync();
            using var command = connection.CreateCommand();
            command.CommandText = "AddProductToWarehouse";
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@IdProduct", productMetadata.IdProduct);
            command.Parameters.AddWithValue("@IdWarehouse", productMetadata.IdWarehouse);
            command.Parameters.AddWithValue("@Amount", productMetadata.Amount);
            command.Parameters.AddWithValue("@CreatedAt", productMetadata.CreatedAt);
            try
            {
                var primaryKey = int.Parse((await command.ExecuteScalarAsync()).ToString());
                return primaryKey;
            }
            catch
            {
                return -1;
            }
        }
    }
}

