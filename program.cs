using System;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics.Arm;

// https://learn.microsoft.com/en-us/training/modules/challenge-project-work-variable-data-c-sharp/2-prepare

//Clonable class to make solution scalable and easier to read and understand since we can now
//define each animal in one function when we clone this class.
public class animalFile
{
    //Checks for any values that would leave a blank space on the animalFile. Replaces it with N/A
    private string checkInvalid(string ins)
    {
        if (ins == null || ins.Length == 0 || ins == "\r" || ins == "\n" || ins == " ")
        {
            return "N/A";
        }
        return ins;
    }
    //init class with defs
    public animalFile(string species_, string id_, string age_, string phy_desc_, string per_desc_, string nickname_, string sug_dono_)
    {
        //Assign func args to their respective string vars
        species = checkInvalid(species_);
        ID = checkInvalid(id_);
        age = checkInvalid(age_);
        physicalDescription = checkInvalid(phy_desc_);
        personalityDescription = checkInvalid(per_desc_);
        nickname = checkInvalid(nickname_);

        try
        {
            // Parse the input string sug_dono for a decimal number. If it's NaN, default to 45.00
            if (!decimal.TryParse(sug_dono_, out decimalDonation))
            {
                decimalDonation = 45.00m; // if suggestedDonation NOT a number, default to 45.00
            }
            suggestedDonation = $"{decimalDonation:C2}"; //Parsed decimal back to string
        }
        catch (FormatException ex)
        {
            // Handle parsing exceptions
            Console.WriteLine($"Error parsing donation: {ex.Message}. Using default value.");
            decimalDonation = 45.00m;
            suggestedDonation = $"{decimalDonation:C2}";
        }
    }

    // ourAnimals class will store the following: 
    public string species { get; private set; }
    public string ID { get; private set; }
    public string age { get; private set; }
    public string physicalDescription { get; private set; }
    public string personalityDescription { get; private set; }
    public string nickname { get; private set; }
    public string suggestedDonation { get; private set; }

    //Decimal donation number used in the initializing func. Set to private since
    //it has no use being called outside the initializing func.
    private decimal decimalDonation = 0.00m;
}

//Main program class
class Program
{
    //Take input func that trims and lowercases every input
    private static void takeInput(ref string? input)
    {
        string? readResult = null; //Data entry for user input

        //Request input from user for the desired species
        while (readResult == null)
        {
            readResult = Console.ReadLine();
            if (readResult != null)
            {
                input = readResult.ToLower().Trim();
                Console.WriteLine();
            }
        }
    }

    //Take all animals in an array and format their characteristics into a single string
    //Which is then returned to the caller to be handeled from there.
    private static string listAllAnimals(animalFile[] animalsArr)
    {
        return string.Join("\n\n", animalsArr.Select(animal_ =>
        $"Species: {animal_.ID}\nID#: {animal_.species}\nAge: {animal_.age}\nNickname: {animal_.nickname}\n" +
        $"Physical description: {animal_.physicalDescription}\nPersonality: {animal_.personalityDescription}\n" +
        $"Suggested Donation: {animal_.suggestedDonation:C2}\n"));
    }

    //Search an animal by ID... If not found then returns with no results found for ID#
    private static string searchID(animalFile[] animalsArr, string ID)
    {
        for (int i = 0; i<animalsArr.Count(); i++) 
        {
            if (animalsArr[i].ID == ID)
            {
                return $"Species: {animalsArr[i].ID}\nID#: {animalsArr[i].species}\nAge: {animalsArr[i].age}\n" +
                       $"Nickname: {animalsArr[i].nickname}\nPhysical description: {animalsArr[i].physicalDescription}\n" +
                       $"Personality: {animalsArr[i].personalityDescription}\nSuggested Donation: {animalsArr[i].suggestedDonation}\n\n";
            };
        };

        return $"No results found for ID#: {ID}.";
    }

    //Searches a given species for characteristics...can also just search all
    private static string searchCharacteristics(animalFile[] animalArr, string species, string[] characteristics)
    {
        string? matchingNicknames = null;

        //Nested for loop...
        //First loop is to check if an animal is of the correct species
        for (int i = 0; i < animalArr.Count(); i++)
        {
            //Check if the species contains the species requested. also lowercases it.
            if (animalArr[i].species.Contains(species.ToLower()) || species == "all")
            {
                //If true then next check check if the species has any of the characteristics desired
                for (int y = 0; y < characteristics.Count(); y++)
                {
                    //Get the index of the match
                    int physicalIndex = animalArr[i].physicalDescription.IndexOf(characteristics[y]);
                    int personalIndex = animalArr[i].personalityDescription.IndexOf(characteristics[y]);
                    //If there is no match then the index will be -1

                    //Check Physical & Personality description for requested characteristics
                    if (physicalIndex > -1 || personalIndex > -1)
                    {
                        /* Checking to make sure the words are whole
                         * It could check for the word male and declare female as a match because it contains male
                         * This is solved by checking if the index of the match has a space before it or not
                         * If the index isnt at the beginning of the str it will subtract 1 from the index and compare
                         * it to ' ' if its a match then its valid, if not then its invalid and we dont add it.
                         */
                        if ((physicalIndex == 0 || personalIndex == 0) ||
                            (physicalIndex > 0 && animalArr[i].physicalDescription[physicalIndex - 1] == ' ') ||
                            (personalIndex > 0 && animalArr[i].personalityDescription[personalIndex - 1] == ' '))
                        {
                            matchingNicknames += $"{animalArr[i].nickname} (ID#: {animalArr[i].ID}): {characteristics[y]}\n";
                        };
                    };
                }
            }
        }

        //If there were no matches then write this line as a fallback
        if (matchingNicknames == null)
        {
            matchingNicknames = "There are no matches for the species \"" + species + "\" and the characteristics ";
            for (int x = 0; x < characteristics.Count(); x++)
            {
                //Ternary operator to decide if to add ", " or not.
                matchingNicknames += (x == characteristics.Count() - 1 ? "& \"" + characteristics[x] + "\"." : "\"" + characteristics[x]+ "\"" + ", ");
            };
        }

        return matchingNicknames;
    }

    //Main function
    public static void Main(string[] args)
    {
        ///////////////////////
        //Create Animal Files//
        ///////////////////////

        animalFile lola = new animalFile(
            "dog", 
            "d1", 
            "2", 
            "medium sized cream colored female golden retriever weighing about 45 pounds. housebroken.", 
            "loves to have her belly rubbed and likes to chase her tail. gives lots of kisses.", 
            "lola", 
            "85.00");

        animalFile gus = new animalFile(
            "dog",
            "d2",
            "9",
            "large reddish-brown male golden retriever weighing about 85 pounds. housebroken.",
            "loves to have his ears rubbed when he greets you at the door, or at any time! loves to lean-in and give doggy hugs.",
            "gus",
            "49.99");

        animalFile snow = new animalFile(
            "cat",
            "c3",
            "1",
            "small white female weighing about 8 pounds. litter box trained.",
            "friendly",
            "snow",
            "40.00");

        animalFile lion = new animalFile(
            "cat",
            "c4",
            "",
            "",
            "",
            "lion",
            "");

        animalFile[] animals = { lola, gus, snow, lion }; //Array to store all instances of an animal


        ///////////////////////
        //Additional Defs    //
        ///////////////////////

        string? menuSelection = "";

        ///////////////////////
        //Console Loop       //
        ///////////////////////

        do
        {
            // NOTE: the Console.Clear method is throwing an exception in debug sessions
            Console.Clear();

            Console.WriteLine("Welcome to Hejlsberg's PetFriends app. Your main menu options are:");
            Console.WriteLine(" 1. List all of our current pet information");
            Console.WriteLine(" 2. Search a species with specified characteristics");
            Console.WriteLine(" 3. Search pets via ID");
            Console.WriteLine(" 4. Add animal to database");
            Console.WriteLine();
            Console.WriteLine("Enter your selection number (or type Exit to exit the program)");

            menuSelection = null;
            takeInput(ref menuSelection);

            //Switch case on user input
            switch (menuSelection)
            {
                //case 1, list all animals in the animals array and all their characteristics
                case "1":
                    Console.WriteLine(listAllAnimals(animals));

                    Console.WriteLine();
                    Console.WriteLine("Press enter to return to main menu");
                    Console.ReadLine();
                    break;

                //Parse array for dogs and search for multiple characteristics
                case "2":
                    string? desiredSpecies = null;
                    
                    //Request input from user for the desired species
                    Console.WriteLine($"\r\nEnter one desired species to search for... Or type \"all\" to search all species.");
                    takeInput(ref desiredSpecies);

                    string? inputCharacteristics = null;

                    //Request input for the desired characteristics
                    Console.WriteLine($"\r\nEnter desired characteristics to search for seperated by commas");
                    takeInput(ref inputCharacteristics);

                    //LINQ way of getting char count of commas
                    int count = inputCharacteristics.Count(c => c == ','); ++count; //3 terms would have 2 commas, therefor add another to count to compensate for that

                    //Make an array that is the size of the amount of single word characteristics
                    string[] inCharsArr = new string[count];

                    //Parse the string for the relevant data
                    //Using ',' as the seperator, add each letter to the array pos until we reach ' '
                    //Then skip the ',' and increment the array to the next slot.
                    for (int i = 0, j = 0; j < inputCharacteristics.Length; j++)
                    {
                        if (inputCharacteristics[j] == ',') { 
                            inCharsArr[i].Trim(); 
                            i++; 
                            if (inputCharacteristics[j+1] == ' ') { j++; };
                        }
                        else { inCharsArr[i] += inputCharacteristics[j]; };
                    }

                    Console.WriteLine("Here are the animals that are of the species \"" + desiredSpecies + "\" and with the characteristics \"" + inputCharacteristics + "\"");
                    Console.WriteLine();

                    //Finally, call the searchCharacteristics func and write to console
                    Console.WriteLine(searchCharacteristics(animals, desiredSpecies, inCharsArr));
                    Console.WriteLine();

                    Console.WriteLine("Press enter to return to main menu");
                    Console.ReadLine(); //Use as a pause buffer until next enter stroke
                    break;

                case "3":
                    string? inpID = null;
                    Console.WriteLine("Please input the ID you'd like to search for.");
                    takeInput(ref inpID);

                    Console.WriteLine(searchID(animals, inpID));

                    Console.WriteLine();
                    Console.WriteLine("Press enter to continue");
                    Console.ReadLine();
                    break;

                case "4":
                    string? inSpecies = null, inID = null, 
                        inAge = null, inName = null, 
                        inPhy = null, inPer = null, inDono = null;

                    Console.WriteLine("Please input the animal's species");
                    takeInput(ref inSpecies);

                    Console.WriteLine("Please input the animal's ID");
                    takeInput(ref inID);

                    bool dup = animals.Any(a => a.ID == inID);
                    //If there is a matching ID then do not add animal
                    if (dup) 
                    {
                        Console.WriteLine("Error, duplicate ID found.");
                        Console.WriteLine("Please try again.");
                        Console.WriteLine();
                        Console.WriteLine("Press enter to continue");
                        Console.ReadLine();

                        break; 
                    }; //Exit switch case if duplicate found

                    Console.WriteLine("Please input the animal's Age");
                    takeInput(ref inAge);

                    Console.WriteLine("Please input the animal's Name");
                    takeInput(ref inName);

                    Console.WriteLine("Please input the animal's Physical Description");
                    takeInput(ref inPhy);

                    Console.WriteLine("Please input the animal's Personality");
                    takeInput(ref inPer);

                    Console.WriteLine("Please input the animal's suggested Donation");
                    takeInput(ref inDono);

                    animalFile newAnimal = new animalFile(inSpecies, inID, inAge, inPhy, inPer, inName, inDono);
                    
                    Array.Resize(ref animals, animals.Length+1);
                    animals[animals.Length-1] = newAnimal;

                    GC.Collect(); //Force memory clean
                    break;
            };
        } while (menuSelection != "exit");
    }
}
