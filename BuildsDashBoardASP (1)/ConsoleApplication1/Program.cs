using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Build.Client;
using System.Security.Principal;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.Lab.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Text;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;



namespace ConsoleApplication1
{
    class Program
    {
        public class Build
        {
            public int id { get; set; }
            public string name { get; set; }
            public string requestedBy { get; set; }
            public string hasDiagnostics { get; set; }
            public string uri { get; set; }
            public string definition { get; set; }
            public string dropFolder { get; set; }
            public string text { get; set; }
            public string type { get; set; }
            public string localPath { get; set; }
            public string quality { get; set; }
            public string status { get; set; }
            public string finished { get; set; }
            
            //public decimal Price { get; set; }
        }
        static void Main(string[] args)
        {
            //int i = 0;
            //RunAsync().Wait();
            string[] list = {"Yuri", "Interview", "Nordstrom", "Cat", "Dog", "Telephone", "AVeryLongString", "This code puzzle is easy"};
            Console.Write( isPermutation("aei", "iea"));
            Console.WriteLine( getLongestString(1, list.ToList()));

          Console.Read();
        }


        static string getLongestString(int nTh, List<string> inputs)
        {
            if (nTh < 1)
                throw new Exception("too small rank");
            if (nTh > inputs.Count)
                throw new Exception("too small rank");

	            //sort the strings by character count in decreasing order
            inputs = inputs.OrderByDescending(s => s.Length).ToList();

            return inputs[nTh - 1];
	//store the strings in dictionary<int,String>
        }
        static private bool isPermutation(string myString1, string myString2)
        {
            //If the strings are different lengths, they are not
            //permutations.
            if (myString1.Length != myString2.Length) return false;

            //Create an array to count the number of each specific
            //character in the strings.
            int[] characterCount = new int[256];
            int charIndex;

            //Populate the array with default value 0.
            for (int index = 0; index < 256; index++)
            {
                characterCount[index] = 0;
            }

            //Count the number of each character in the first
            //string. Add the count to the array.
            foreach (char myChar in myString1.ToCharArray())
            {
                charIndex = (int)myChar;
                characterCount[charIndex]++;
            }

            //Count the number of each character in the second
            //string. Subtract the count from the array.
            foreach (char myChar in myString2.ToCharArray())
            {
                charIndex = (int)myChar;
                characterCount[charIndex]--;
            }

            //If the strings are permutations, then each character
            //would be added to our character count array and then
            //subtracted. If all values in this array are not 0
            //then the strings are not permutations of each other.
            for (int index = 0; index < 256; index++)
            {
                if (characterCount[index] != 0) return false;
            }

            //The strings are permutations of each other.
            return true;
        }

        /// <summary>
        /// ///////
        /// </summary>
        /// <returns></returns>
        static async Task RunAsync()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://adsgroupvstf:8080/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // HTTP GET
                HttpResponseMessage response = await client.GetAsync("tfs/AdsGroup/AdsApps/_api/_build/completedBuilds");
                if (response.IsSuccessStatusCode)
                {
                    Byte[] builds = await response.Content.ReadAsByteArrayAsync();
                    //List<Build> builds = await response.Content.ReadAsAsync<List<Build>>();
                    //foreach (Build b in builds)
                   // Console.WriteLine("{0}\t${1}\t{2}", b.id, b.name, b.definition);
                }

                //// HTTP POST
                //var gizmo = new Product() { Name = "Gizmo", Price = 100, Category = "Widget" };
                //response = await client.PostAsJsonAsync("api/products", gizmo);
                //if (response.IsSuccessStatusCode)
                //{
                //    Uri gizmoUrl = response.Headers.Location;

                //    // HTTP PUT
                //    gizmo.Price = 80;   // Update price
                //    response = await client.PutAsJsonAsync(gizmoUrl, gizmo);

                //    // HTTP DELETE
                //    response = await client.DeleteAsync(gizmoUrl);
                //}
            }
        }
    }
}
