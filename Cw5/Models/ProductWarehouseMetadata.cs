﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Cw5
{
	public class ProductWarehouseMetadata
	{
		public int IdWarehouse { get; set; }
		public int IdProduct { get; set; }
		public int IdOrder { get; set; }
		public int Amount { get; set; }
		public double Price { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}

