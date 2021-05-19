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

            SecurityManager.CreatePinHash(ref cards,ref wallet);

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

            TheActualProgram(ref cards, ref wallet, ATMoptions);//Yea lets not talk about the method naming

        }

        static void TheActualProgram(ref List<CardModel> cards, ref WalletModel wallet, IDictionary<int, string> ATMoptions)
        {
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

                if (!SecurityManager.IsPinValid(ref cardHasBeenEntered, ref chosenCard))
                {
                    if (!cardHasBeenEntered || chosenCard.IsCardLocked) continue;
                }

                chosenCard.NumberOfFailedAttempts = 0;

                SelectOptions(ref cardHasBeenEntered, ATMoptions, ref chosenCard, ref wallet);
            }
        }

        static void SelectOptions(ref bool cardHasBeenEntered, IDictionary<int, string> ATMoptions, ref CardModel chosenCard, ref WalletModel wallet)
        {
            string input;
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
                if (!validChosenAction) continue;
                // If the chosen value is value go into the specifics of the options
                ChoosenActionSwitch(chosenAction, ref cardHasBeenEntered, ref chosenCard, ref wallet);
            }
        }
        static void ChoosenActionSwitch(int chosenAction, ref bool cardHasBeenEntered, ref CardModel chosenCard, ref WalletModel wallet)
        {
            string input;
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
                    //This could be put into a method but it would be harder to read, lets leave it here
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
                    //This could be put into a method but it would be harder to read, lets leave it here
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
