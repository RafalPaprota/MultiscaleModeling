using GrainGrowth.Lib.Extensions;
using GrainGrowth.Lib.Models;
using Grains.Lib.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grains.Library.Extensions
{
    public static class MonteCarloExtensions
    {
        public static void GenerateMonteCarloArea(this Grid grid, int number)
        {
            var random = new Random();

            for (int i = 0; i < grid.Width; i++)
            {
                for (int j = 0; j < grid.Height; j++)
                {
                    if (grid.Cells[i, j] == 0)
                    {
                        grid.Cells[i, j] = random.Next(number) + 2;
                    }
                }
            }
        }

        public static void MonteCarloStep(this Grid grid, double jb)
        {
            var random = new Random();
            var notVisitedCells = new List<Cell>(grid.CellsWhereBorderId);
            notVisitedCells.Shuffle(random);

            foreach (var currentCell in notVisitedCells)
            {
                currentCell.Id = grid.Cells[currentCell.X, currentCell.Y];

                var currentEnergy = grid.CalculateEnergy(currentCell, jb);

                if (currentEnergy == 0)
                {
                    continue;
                }

                var currentCellValue = currentCell.Id;
                var tempCellValue = random.Next(grid.IdsNumber) + 2;

                if (tempCellValue == currentCellValue)
                {
                    continue;
                }

                currentCell.Id = tempCellValue;

                var newEnergy = grid.CalculateEnergy(currentCell, jb);

                if (newEnergy < currentEnergy)
                {
                    grid.Cells[currentCell.X, currentCell.Y] = tempCellValue;
                }
            }
        }

        public static void SRXMonteCarloStep(this Grid grid, double jb, NucleationModuleType nucleationType, NucleationArea nucleationArea, int nucleationSize, int currentStep)
        {
            switch (nucleationType)
            {
                case NucleationModuleType.Constant:
                    {
                        grid.AddRecrystalisedNucleons(nucleationSize, nucleationArea);
                        break;
                    }

            }

            var random = new Random();
            var notVisitedCells = new List<Cell>(grid.ShuffledCells);

            foreach (var currentCell in notVisitedCells)
            {
                currentCell.Id = grid.Cells[currentCell.X, currentCell.Y];

                var randomNeighbour = grid.GetRandomNeighbour(currentCell, random);

                if (grid.Energy[randomNeighbour.X, randomNeighbour.Y] != 0)
                {
                    continue;
                }

                var currentEnergy = grid.CalculateEnergy(currentCell, jb) + grid.Energy[currentCell.X, currentCell.Y];

                if (currentEnergy == 0)
                {
                    continue;
                }

                var currentCellValue = currentCell.Id;

                var tempCellValue = grid.Cells[randomNeighbour.X, randomNeighbour.Y];

                if (tempCellValue == currentCellValue)
                {
                    continue;
                }

                currentCell.Id = tempCellValue;

                var newEnergy = grid.CalculateEnergy(currentCell, jb);

                if (newEnergy < currentEnergy)
                {
                    grid.Cells[currentCell.X, currentCell.Y] = tempCellValue;
                    grid.Energy[currentCell.X, currentCell.Y] = 0;
                }
            }
        }


        private static int CalculateEnergy(this Grid grid, Cell cell, double j)
        {
            int kroeneckerDelta = 0;
            var coordinates = Unites.Unites.MooreCoordinates;

            foreach (var point in coordinates)
            {
                var tempCell = cell.Get(point.X, point.Y).ReworkeCell(grid);
                var tempId = grid.Cells[tempCell.X, tempCell.Y];
                if (tempId != cell.Id)
                {
                    kroeneckerDelta += 1;
                }
            }

            return kroeneckerDelta;
        }



            private static Cell GetRandomNeighbour(this Grid grid, Cell cell, Random random)
        {
            var coordinates = Unites.Unites.MooreCoordinates;
            var neighbours = new List<Cell>();

            foreach (var point in coordinates)
            {
                var tempCell = cell.Get(point.X, point.Y).ReworkeCell(grid);
                neighbours.Add(tempCell);
            }

            return neighbours.ElementAt(random.Next(neighbours.Count));
        }
    }
}
