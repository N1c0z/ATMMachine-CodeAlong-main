using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ATMMachine
{
    public class CardModel
    {
        public int CardId { get; set; }
        public string BankName { get; set; }
        public int AccountNumber { get; set; }
        public List<byte> PIN { get; set; }
        public double Balance { get; set; }
        public string PasswordHash { get; set; }

        public int NumberOfFailedAttempts { get; set; }

        public bool IsCardLocked { get; set; }

        public CardModel(int cardId, int accountNumber, List<byte> pin, double balance, string bankName, string passwordHash, int numberOfFailedAttempts, bool isCardLocked)
        {
            CardId = cardId;
            AccountNumber = accountNumber;
            PIN = pin;
            Balance = balance;
            BankName = bankName;
            PasswordHash = passwordHash;
            NumberOfFailedAttempts = numberOfFailedAttempts;
            IsCardLocked = isCardLocked;
        }
    }
}
