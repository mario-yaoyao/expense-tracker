using BudgetWise.BLL.Interfaces;
using BudgetWise.DAL.Interfaces;
using BudgetWise.Models.Dtos.Requests;
using BudgetWise.Models.Models;

namespace BudgetWise.BLL.Jobs
{
    public class RecurringTransactionJob(IIncomeService incomeService, IExpenseService expenseService, IRecurringTransactionRepository recurringTransactionsRepository) : IRecurringTransactionJob
    {
        public async Task ProcessRecurringTransactionsAsync()
        {
            var recurringTransactions = await recurringTransactionsRepository.GetActiveRecurringTransactionsAsync();

            foreach (var recurringTransaction in recurringTransactions)
            {
                if (recurringTransaction.Type == CategoryType.Income)
                {
                    await incomeService.CreateRecurringIncomeAsync(
                        recurringTransaction.UserId,
                        recurringTransaction.Description,
                        recurringTransaction.Amount,
                        recurringTransaction.CategoryId,
                        recurringTransaction.Id);
                }
                else
                {
                    await expenseService.CreateRecurringExpenseAsync(
                        recurringTransaction.UserId,
                        recurringTransaction.Description,
                        recurringTransaction.Amount,
                        recurringTransaction.CategoryId,
                        recurringTransaction.Id);
                }
            }
        }
    }
}
