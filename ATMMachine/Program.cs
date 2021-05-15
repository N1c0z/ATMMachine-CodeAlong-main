using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace ATMMachine
{
    class Program
    {
        static void Main(string[] args)
        {

            //Get our data, Creating the cards.
            List<CardModel> cards = new List<CardModel>()
            {
                new CardModel(1, 12345, null, 250.00, "Lloyds", string.Empty, 0, false),
                new CardModel(2, 55555, null, 100.50, "HSBC", string.Empty, 0, false),
                new CardModel(3, 99999, null, 1000, "Bank Of England", string.Empty, 0, false),
            };

            WalletModel wallet = new WalletModel(balance: 230);
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
            Console.WriteLine("Clear console to hide passwords? Y/N");
            if (Console.ReadLine().ToLower().Contains("y")) Console.Clear();


            //Create the list of options used on line 55
            IDictionary<int, string> ATMoptions = new Dictionary<int, string>
            {
                {1, "Cancel" },
                {2, "View Balance" },
                {3, "Withdraw Wibbly Dollars To Wallet" },
                {4, "Deposit Wibbly Dollars From Wallet" }
            };

            //This ensures the app is always running
            Console.WriteLine("To return here just write \"cancel\" anywhere");
            while (true)
            {
                bool cardHasBeenEntered = false;

                Console.WriteLine("Free Cash Withdrawal, Please Enter a Card...");

                //This is a discard, once the try prase returns true it will discard the returned value
                if (!int.TryParse(Console.ReadLine(), out int enteredCardId))
                {
                    Console.WriteLine("Value inserted not accepted, please check you typed everything right and try again");
                    continue;
                }

                //Find the card with the entered cardId from the above ReadLine()
                CardModel chosenCard = cards.Find(c => c.CardId == enteredCardId);


                //If card is equal to null, meaning it hasnt found the card entered, dont go to the next step
                if (chosenCard == null)
                {
                    Console.WriteLine("Card inserted not found, please try with another card\n");
                    continue;
                }
                if (chosenCard.IsCardLocked)
                {
                    Console.WriteLine("Card has been locked due to security reasons, please choose another card\n");
                    continue;
                }
                //set the card entered to true and go to next step
                cardHasBeenEntered = true;

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

                if (!cardHasBeenEntered || chosenCard.IsCardLocked) continue;

                chosenCard.NumberOfFailedAttempts = 0;

                while (cardHasBeenEntered)
                {
                    //Tell the user to chose from a list of options
                    Console.WriteLine("Choose from a list of options....");

                    //loop through the options declared on line 21, and print
                    foreach (KeyValuePair<int, string> option in ATMoptions)
                    {
                        Console.WriteLine($"{option.Key}: - {option.Value}");
                    }

                    input = Console.ReadLine();
                    if (input.ToLower().Contains("cancel"))
                    {
                        cardHasBeenEntered = false;
                        break;
                    }
                    bool validChosenAction = int.TryParse(input, out int chosenAction);

                    // If the chosen value is value go into the specifics of the options
                    if (validChosenAction)
                    {
                        switch (chosenAction)
                        {
                            case 1:
                                //returns the user to the welcome page and clears the console
                                cardHasBeenEntered = false;
                                Console.Clear();
                                break;

                            case 2:
                                //Displays the value of the current card/account
                                Console.WriteLine($"W: {chosenCard.Balance} Wibbly Dollars.\nWallet balance: {wallet.Balance}");
                                break;

                            case 3:
                                while (true)
                                {
                                    //ask the user to enter an amount to withdraw
                                    Console.WriteLine($"Select an amount to withdraw...");
                                    input = Console.ReadLine();
                                    if (input.ToLower().Contains("cancel"))
                                    {
                                        cardHasBeenEntered = false;
                                        break;
                                    }
                                    if (int.TryParse(input, out int ammount))
                                    {
                                        if (ammount < 0)
                                        {
                                            Console.WriteLine("Inserted value can not be negative\n");
                                        }
                                        else
                                        {
                                            WithdrawWibblyDollars(ammount, chosenCard, wallet);
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Inserted value isnt acccepted, try again\n");
                                    }
                                }
                                break;
                            case 4:
                                while (true)
                                {
                                    //ask the user to enter an amount to withdraw
                                    Console.WriteLine($"Select an amount to deposit...");
                                    input = Console.ReadLine();
                                    if (input.ToLower().Contains("cancel"))
                                    {
                                        cardHasBeenEntered = false;
                                        break;
                                    }
                                    if (int.TryParse(input, out int ammount))
                                    {
                                        if (ammount < 0)
                                        {
                                            Console.WriteLine("Inserted value can not be negative\n");
                                        }
                                        else
                                        {
                                            DepositWibblyDollars(ammount, chosenCard, wallet);
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Inserted value isnt acccepted, try again\n");
                                    }
                                }
                                break;
                            default:
                                Console.WriteLine($"Inserted value isnt acccepted, try again\n");
                                break;
                        }
                    }
                }
            }

        }
        static void DepositWibblyDollars(int amount, CardModel chosenCard, WalletModel wallet)
        {
            string message = "Not enough funds\n";

            if (amount <= wallet.Balance)
            {
                message = $"You deposited {amount} Wibbly Dollars";
                chosenCard.Balance += amount;
                wallet.Balance -= amount;
            }
            Console.WriteLine(message);
        }
        static void WithdrawWibblyDollars(int amount, CardModel chosenCard, WalletModel wallet)
        {
            //sets the message to not enough funds
            string message = "Not enough funds\n";

            //if the condition is true change the message and subtract the amount from the balance
            if (amount <= chosenCard.Balance)
            {
                message = $"You withdrew {amount} Wibbly Dollars";
                chosenCard.Balance -= amount;
                wallet.Balance += amount;
            }

            Console.WriteLine(message);
        }
    }
}
