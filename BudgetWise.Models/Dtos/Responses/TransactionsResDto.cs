namespace BudgetWise.Models.Dtos.Responses
{
    public class TransactionsResDto
    {
        public List<TransactionResDto> Items { get; set; } = [];
        public PaginatedResDto Pagination { get; set; } = new();
    }
}
