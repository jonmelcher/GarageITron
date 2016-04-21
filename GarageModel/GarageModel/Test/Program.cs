using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GarageModel;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            GarageAssignment assignment = GarageRepository.GetGarageAssignment("39009D3D68");
            Console.WriteLine($"{assignment.ID} {assignment.Stored} {assignment.Tier} {assignment.Cell}");
            GarageRepository.MoveVehicle("39009D3D68", false);
            assignment = GarageRepository.GetGarageAssignment("39009D3D68");
            Console.WriteLine($"{assignment.ID} {assignment.Stored} {assignment.Tier} {assignment.Cell}");
            Console.ReadKey();
        }
    }
}
