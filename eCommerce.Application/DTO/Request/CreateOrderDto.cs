using System;
using System.Collections.Generic;
using System.Linq;

namespace eCommerce.Application.DTO.Request
{
    public class CreateOrderDto
    {
        public int UserId { get; set; }
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }

    public class CreateOrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
