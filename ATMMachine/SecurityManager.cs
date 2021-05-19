using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ATMMachine
{
    public static class SecurityManager
    {
        public static void CreatePinHash(ref List<CardModel> cards, ref WalletModel wallet)
        {
            int pin;
            for (int i = 0; i < cards.Count; i++)
            {
                Console.Write($"Insert pin for your card number {cards.ElementAt(i).CardId} registered with the bank {cards.ElementAt(i).BankName}: ");
                if (!int.TryParse(Console.ReadLine(), out pin))
                {
                    Console.WriteLine("Value inserted not accepted, try again");
                    i--;
                    continue;
                }
                byte[] salt;
                new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

                var bytes = BitConverter.GetBytes(pin);

                var pbkdf2 = new Rfc2898DeriveBytes(bytes, salt, 100000);
                byte[] hash = pbkdf2.GetBytes(20);

                byte[] hashBytes = new byte[36];
                Array.Copy(salt, 0, hashBytes, 0, 16);
                Array.Copy(hash, 0, hashBytes, 16, 20);

                cards.ElementAt(i).PasswordHash = Convert.ToBase64String(hashBytes);
            }
            pin = 0;
        }

        public static bool IsPinValid(ref bool cardHasBeenEntered, ref CardModel chosenCard)
        {
            string input;
            int InsertedPin;
            byte[] intBytes;
            bool correctPassword;
            while (true)
            {
                correctPassword = true;
                Console.WriteLine("Insert the pin number of the selected card");
                input = Console.ReadLine();
                if (input.ToLower().Contains("cancel"))
                {
                    cardHasBeenEntered = false;
                    break;
                }
                if (!int.TryParse(input, out InsertedPin))
                {
                    Console.WriteLine("Inserted value not accepted, please try again\n");
                    continue;
                }
                intBytes = BitConverter.GetBytes(InsertedPin);

                string savedPasswordHash = chosenCard.PasswordHash;

                byte[] hashBytes = Convert.FromBase64String(savedPasswordHash);

                byte[] salt = new byte[16];

                Array.Copy(hashBytes, 0, salt, 0, 16);

                var pbkdf2 = new Rfc2898DeriveBytes(intBytes, salt, 100000);

                byte[] hash = pbkdf2.GetBytes(20);

                for (int i = 0; i < 20; i++)
                    if (hashBytes[i + 16] != hash[i])
                    {
                        correctPassword = false;

                        chosenCard.NumberOfFailedAttempts += 1;

                        Console.WriteLine($"Inserted pin is incorrect, amount of tries remaining {3 - chosenCard.NumberOfFailedAttempts}\n");

                        if (chosenCard.NumberOfFailedAttempts == 3)
                        {
                            chosenCard.IsCardLocked = true;

                            Console.WriteLine("Card has been locked since there have been 3 failed attempts\n");
                        }
                        break;
                    }
                if (chosenCard.IsCardLocked || correctPassword) break;
            }
            if (chosenCard.IsCardLocked || !cardHasBeenEntered) return false;
            else return true;
        }

    }
}
