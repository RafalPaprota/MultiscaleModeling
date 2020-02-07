

using GrainGrowth.Lib.Enums;
using GrainGrowth.Lib.Models;
using Grains.Lib.Enums;
using Grains.Library.Enums;
using Grains.Library.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrainGrowth.Lib.builders
{
    public class builder
    {
        private Grid grid1;
        private Grid grid2;

        private int width;
        private int height;
        public bool EnergySpread;
        public int MaxCellIdNumber => this.grid1.Cells.Max();
        public List<Cell> UpdatedCells;
        public int CurrentStep = 0;
        public int[,] Array => grid1.Cells;
        public int[,] Energy => grid1.Energy;
        public int StepsLimit = 150;
        private Neighbourhood Neighbourhood;
        public delegate void StepIncrementedDelegate(int stepnumber);
        public event StepIncrementedDelegate StepAdding;

        public SimulationType TypesOfSimulation { get; set; }
        public NucleationModuleType TypeOfNucleation { get; set; }
        public NucleationArea NucleSize { get; set; }
        public int NucleArea { get; set; }



        public builder(int width, int height)
        {
            grid1 = new Grid(width, height);
            grid2 = new Grid(width, height);

            this.width = width;
            this.height = height;
  
            TypesOfSimulation = SimulationType.MonteCarlo;
        }

        public async Task ClearAllButBorders()
        {
            await Task.Run(() => grid1.ClearAllButBorders());
            OverwriteGrid(grid1, grid2);
        }

        public async Task AddRandomGrains(int amount)
        {
            await Task.Run(() => grid1.AddRandomGrains(amount));
            OverwriteGrid(grid1, grid2);
        }

        public async Task AddInclusions(int amount, int size, Inclusions type)
        {
            await Task.Run(() => grid1.AddInclusions(amount, size, type));
            OverwriteGrid(grid1, grid2);
        }

        public void SetNeighbourhood(Neighbourhood neighbourhood)
        {
            this.Neighbourhood = neighbourhood;
        }

        public async Task AddRecrystalisedNucleons()
        {
            await Task.Run(() => this.grid1.AddRecrystalisedNucleons(NucleArea, NucleSize));
            OverwriteGrid(grid1, grid2);
        }

        public async Task CreateSubstructure(Substructures substructure, int grains)
        {
            await Task.Run(() => this.grid1.SubstructueCreation(substructure, grains));
            OverwriteGrid(grid1, grid2);
        }


        public void SetBorderStyle(BorderStyle borderStyle)
        {
            grid1.Border = borderStyle;
            grid2.Border = borderStyle;
        }

        public async Task Clear()
        {
            await Task.Run(() =>
            {
                grid1 = new Grid(width, height);
                grid2 = new Grid(width, height);
            });
        }

        public async Task DistributeEnergy(EnergySpreadType energyDistributionType)
        {
            await Task.Run(() => this.grid1.DistributeEnergy(energyDistributionType));
            EnergySpread = true;
            OverwriteGrid(grid1, grid2);
        }

        public void StartGrowth()
        {

        }

        public void Step(int x, double j)
        {
            switch (TypesOfSimulation)
            {
                case SimulationType.CellularAutomata:
                    {
                        grid2.CerrularAutmataStep(grid1, Neighbourhood, x);
                        break;
                    }
                case SimulationType.MonteCarlo:
                    {
                        grid2.MonteCarloStep(j);
                        break;
                    }
                case SimulationType.SRXMonteCarlo:
                    {
                        grid2.SRXMonteCarloStep(j, TypeOfNucleation, NucleSize, NucleArea, CurrentStep);
                        CurrentStep++;
                        StepAdding(CurrentStep);
                        break;
                    }
            }

            OverwriteGrid(grid2, grid1);
        }

        public async Task AddBorders(int size)
        {
            await Task.Run(() => grid1.AddBorders(size));
            OverwriteGrid(grid1, grid2);
        }

        public async Task ClearEnergy()
        {
            await Task.Run(() => this.grid1.ClearEnergy());
            OverwriteGrid(grid1, grid2);
        }

        public async Task GenerateMonteCarloArea(int number)
        {
            await Task.Run(() => this.grid1.GenerateMonteCarloArea(number));

            OverwriteGrid(grid1, grid2);
        }




        private void OverwriteGrid(Grid source, Grid target)
        {
            target.ReservationId = source.ReservationId;

            Parallel.For(0, source.Width, i =>
            {
                Parallel.For(0, source.Height, j =>
                {
                    target.Cells[i, j] = source.Cells[i, j];
                    target.Energy[i, j] = source.Energy[i, j];
                    target.NotEmptyCells[i, j] = source.NotEmptyCells[i, j];
                });
            });
        }
    }
}
