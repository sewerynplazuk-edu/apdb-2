using System;
using System.ComponentModel.DataAnnotations;

namespace Cw4.Models
{
	public class SomeKindOfProduct
	{
		[Required]
		public int IdProduct { get; set; }
		[Required]
		public int IdWarehouse { get; set; }
		[Required]
		[Range(1, int.MaxValue)]
		public int Amount { get; set; }
		[Required]
		[DataType(DataType.DateTime)]
		public DateTime CreatedAt { get; set; }
	}
}

