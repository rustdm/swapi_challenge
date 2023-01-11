using System.Text.Json;
using System.Text.Json.Serialization;

HttpClient client = new HttpClient();
string? myUrl = "https://swapi.dev/api/starships/";

List<Starship> starships = new List<Starship>();
Dictionary<string, string> pilots = new Dictionary<string, string>(); // use url value and name as k,v pair in a Dictionary to minimize # of requests

do
{
    try
    {
        await using Stream starshipStream = await client.GetStreamAsync(myUrl);
        var starshipResponsePage = await JsonSerializer.DeserializeAsync<StarshipResponsePage>(starshipStream);
        myUrl = starshipResponsePage.Next;

        foreach (var ship in starshipResponsePage.Results)
        {
            if (decimal.Parse(ship.Length) >= 10)
            {
                Console.WriteLine(ship.Name + "\n\tLength: " + ship.Length);

                if (ship.Pilots.Any())
                {
                    Console.WriteLine("\tPilots:");

                    foreach (var pilot in ship.Pilots)
                    {
                        if (!pilots.TryGetValue(pilot, out string? pilotName))
                        {
                            try
                            {
                                await using Stream peopleStream = await client.GetStreamAsync(pilot);
                                var peopleResponsePage = await JsonSerializer.DeserializeAsync<PeopleResponsePage>(peopleStream);
                                
                                pilotName = peopleResponsePage.Name;
                                pilots.Add(pilot, peopleResponsePage.Name);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("\nThere has been a disturbance in the force... Exception caught.\n\t" + ex.Message);
                            }
                        }

                        Console.WriteLine("\t\t" + pilotName);
                    }
                }

                starships.Add(ship); // add >= 10 length ships for potential later use + debugging
                Console.WriteLine();
            }
        }
    } 
    catch (Exception ex)
    {
        Console.WriteLine("\nThere has been a disturbance in the force... Exception caught.\n\t" + ex.Message);
    }

    if (myUrl == null)
    {
        
        Console.WriteLine("\nEnd of requests. Press any key to exit..."); // wait for key input to close
        Console.ReadKey();
    }

} while (myUrl != null); // while there are still pages left to request


record class StarshipResponsePage(
    [property: JsonPropertyName("count")] int Count,
    [property: JsonPropertyName("next")] string? Next,
    [property: JsonPropertyName("previous")] string? Previous,
    [property: JsonPropertyName("results")] Starship[] Results
);

record class Starship(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("length")] string Length,
    [property: JsonPropertyName("pilots")] string[] Pilots
);

record class PeopleResponsePage(
    [property: JsonPropertyName("name")] string Name
);