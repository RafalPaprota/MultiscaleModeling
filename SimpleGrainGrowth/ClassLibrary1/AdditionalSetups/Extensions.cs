
using GrainGrowth.Lib.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grains.Library.Extensions.Helpers;
using GrainGrowth.Lib.Models;
using GrainGrowth.Lib.Enums;
using Grains.Library.Actions;
using Grains.Lib.Enums;
using GrainGrowth.Lib.Extensions;
using Grains.Library.Enums;
using System.Linq;
namespace Grains.Library.Extensions
{
    public static class ArrayExtensions
    {
        public static int Max(this int[,] array)
        {
            int max = 0;

            foreach (var number in array)
            {
                if (number > max)
                {
                    max = number;
                }
            }

            return max;
        }
    }
}


namespace GrainGrowth.Lib.Extensions
{
    public static class CellExtensions
    {
        public static Cell ReworkeCell(this Cell cell, Grid grid)
        {
            switch (grid.Border)
            {
                case BorderStyle.Closed:
                    {
                        if (cell.X >= grid.Width)
                        {
                            cell.X = grid.Width - 1;
                        }

                        if (cell.Y >= grid.Height)
                        {
                            cell.Y = grid.Height - 1;
                        }

                        if (cell.X < 0)
                        {
                            cell.X = 0;
                        }

                        if (cell.Y < 0)
                        {
                            cell.Y = 0;
                        }

                        break;
                    }

                case BorderStyle.Transient:
                    {
                        if (cell.X >= grid.Width)
                        {
                            cell.X = Math.Abs(grid.Width - cell.X);
                        }

                        if (cell.Y >= grid.Height)
                        {
                            cell.Y = Math.Abs(grid.Height - cell.Y);
                        }

                        if (cell.X < 0)
                        {
                            cell.X = grid.Width + cell.X;
                        }

                        if (cell.Y < 0)
                        {
                            cell.Y = grid.Height + cell.Y;
                        }

                        break;
                    }
                default:
                    {
                        return cell;
                    }
            }

            return cell;
        }
    }
}

public static class ListSetup
{
    public static void Shuffle<T>(this IList<T> ts, Random random)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = random.Next(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }
}




namespace Grains.Library.Extensions
{
    public static class MatrixExtensions
    {
        public static void AddRandomGrains(this Grid grid, int amount)
        {
            var randomCells = new List<Cell>();

            for (int i = 0; i < grid.Width; i++)
            {
                for (int j = 0; j < grid.Height; j++)
                {
                    if (grid.Cells[i, j] != 0)
                    {
                        continue;
                    }

                    randomCells.Add(new Cell(i, j));
                }
            }

            var rnd = new Random();

            for (int i = 0; i < amount; i++)
            {
                int x = rnd.Next(randomCells.Count);

                var newId = i + 2;

                if (grid.ReservationId.Contains(newId))
                {
                    continue;
                }

                randomCells[x].Id = newId;
                grid.Add(randomCells[x]);
                randomCells.RemoveAt(x);
            }
        }

        public static void AddInclusions(this Grid grid, int amount, int size, Inclusions type)
        {
            var borderCellsHelper = new BorderCellsHelpers();
            var inclusionsHelper = new InclusionsHelper();
            var borderCells = borderCellsHelper.GetBorderCells(grid);
            inclusionsHelper.AddInclusions(grid, amount, size, type, borderCells);
        }


        public static void CerrularAutmataStep(this Grid grid, Grid referenceMatrix, Neighbourhood strategy, int x = 100)
        {
            NeighbourhoodCalculation neighbourAction = NeighbourhoodActive.MooreActivation;

            switch (strategy)
            {
                case Neighbourhood.Moore:
                    {
                        neighbourAction = NeighbourhoodActive.MooreActivation;
                        break;
                    }
                case Neighbourhood.VonNeumann:
                    {
                        neighbourAction = NeighbourhoodActive.VonNeumannActivation;
                        break;
                    }
                case Neighbourhood.MooreExtented:
                    {
                        neighbourAction = NeighbourhoodActive.MooreExtented;
                        break;
                    }
            }

            var random = new Random();

            Parallel.For(0, grid.Width, (i) => {
                Parallel.For(0, grid.Height, (j) => {
                    if (!grid.NotEmptyCells[i, j])
                    {
                        neighbourAction(grid,
                                        new Cell(i, j),
                                        referenceMatrix.Cells,
                                        strategy == Neighbourhood.MooreExtented ? random.Next(100) : 1,
                                        x);
                    }
                });
            });
        }


        public static void DistributeEnergy(this Grid grid, EnergySpreadType energySpreadType)
        {
            switch (energySpreadType)
            {
                case EnergySpreadType.Homogeniczna:
                    {
                        for (int i = 0; i < grid.Width; i++)
                        {
                            for (int j = 0; j < grid.Height; j++)
                            {
                                grid.Energy[i, j] = 5;
                            }
                        }

                        break;
                    }
                case EnergySpreadType.Heterogoniczna:
                    {
                        var borderCellsHelper = new BorderCellsHelpers();
                        var borderCells = borderCellsHelper.GetBorderCells(grid);

                        Parallel.ForEach(borderCells, (cell) => { grid.Energy[cell.X, cell.Y] = 7; });
                        Parallel.ForEach(grid.CellsWhereBorderId, (cell) => {
                            if (grid.Energy[cell.X, cell.Y] != 7)
                            {
                                grid.Energy[cell.X, cell.Y] = 2;
                            }
                        });

                        break;
                    }
            }
        }

        public static void ClearEnergy(this Grid grid)
        {
            grid.Energy = new int[grid.Width, grid.Height];
        }


        public static void AddBorders(this Grid grid, int size)
        {
            for (int x = 0; x < size; x++)
            {
                var unite = Unites.Unites.WidenMooreCoordinates(x);

                for (int i = 0; i < grid.Width; i++)
                {
                    for (int j = 0; j < grid.Height; j++)
                    {
                        var actualCell = new Cell(i, j, grid.Cells[i, j]);

                        foreach (var point in unite)
                        {
                            var tempCell = actualCell.Get(point.X, point.Y).ReworkeCell(grid);
                            var tempId = grid.Cells[tempCell.X, tempCell.Y];

                            if (actualCell.Id == 1)
                            {
                                continue;
                            }

                            if (tempId != actualCell.Id && tempId != 1)
                            {
                                grid.Cells[tempCell.X, tempCell.Y] = 1;
                                grid.NotEmptyCells[tempCell.X, tempCell.Y] = true;
                            }
                        }
                    }
                }
            }
        }


        public static void AddSingleBorder(this Grid grid, int size, int x, int y)
        {
            var desiredId = grid.Cells[x, y];

            for (int s = 0; s < size; s++)
            {
                var coordinates = Unites.Unites.WidenMooreCoordinates(s);

                for (int i = 0; i < grid.Width; i++)
                {
                    for (int j = 0; j < grid.Height; j++)
                    {
                        if (grid.Cells[i, j] != desiredId)
                        {
                            continue;
                        }

                        var currentCell = new Cell(i, j, grid.Cells[i, j]);

                        foreach (var point in coordinates)
                        {
                            var tempCell = currentCell.Get(point.X, point.Y).ReworkeCell(grid);
                            var tempId = grid.Cells[tempCell.X, tempCell.Y];

                            if (currentCell.Id == 1)
                            {
                                continue;
                            }

                            if (tempId != currentCell.Id && tempId != 1)
                            {
                                grid.Cells[tempCell.X, tempCell.Y] = 1;
                            }
                        }
                    }
                }
            }
        }

        public static void ClearAllButBorders(this Grid grid)
        {
            Parallel.For(0, grid.Width, (i) => {
                Parallel.For(0, grid.Height, (j) => {
                    if (grid.Cells[i, j] != 1)
                    {
                        grid.Cells[i, j] = 0;
                        grid.NotEmptyCells[i, j] = false;
                    }
                });
            });
        }








        public static void AddRecrystalisedNucleons(this Grid grid, int amount, NucleationArea areaType)
        {
            IList<Cell> cells = new List<Cell>();

            switch (areaType)
            {
                case NucleationArea.GrainBoundaries:
                    {
                        cells = grid.ShuffledBorderCells;
                        break;
                    }

                case NucleationArea.Random:
                    {
                        cells = grid.ShuffledCells;
                        break;
                    }
            }

            var newIdBase = grid.IdsNumber;

            cells = cells.Where(cell => grid.Energy[cell.X, cell.Y] != 0).ToList();

            if (cells.Count == 0 || amount > cells.Count)
            {
                return;
            }

            for (int i = 0; i < amount; i++)
            {
                var cell = cells.ElementAt(i);

                grid.Cells[cell.X, cell.Y] = newIdBase + i + 1;
                grid.Energy[cell.X, cell.Y] = 0;
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

            return kroeneckerDelta; // * j;
        }


        public static void SubstructueCreation(this Grid grid, Substructures substructure, int grains)
        {
            if (grid.NotEmptyCells.Length == 0)
            {
                return;
            }

            var random = new Random();
            var chosenIds = new List<int>();

            for (int i = 0; i < grains; i++)
            {
                var x = random.Next(grid.Width);
                var y = random.Next(grid.Height);

                var id = grid.Cells[x, y];

                if (!chosenIds.Contains(id) && id != 0 && id != 1)
                {
                    chosenIds.Add(id);
                }
                else
                {
                    i--;
                }
            }

            grid.NotEmptyCells = new bool[grid.Width, grid.Height];

            switch (substructure)
            {
                case Substructures.Substructure:
                    {
                        Parallel.For(0, grid.Width, (i) => {
                            Parallel.For(0, grid.Height, (j) => {
                                if (!chosenIds.Contains(grid.Cells[i, j]))
                                {
                                    grid.Cells[i, j] = 0;
                                }
                                else
                                {
                                    grid.NotEmptyCells[i, j] = true;
                                }
                            });
                        });

                        grid.ReservationId.AddRange(chosenIds);
                        break;
                    }
                case Substructures.DualPhase:
                    {
                        Parallel.For(0, grid.Width, (i) => {
                            Parallel.For(0, grid.Height, (j) => {
                                if (!chosenIds.Contains(grid.Cells[i, j]))
                                {
                                    grid.Cells[i, j] = 0;
                                }
                                else
                                {
                                    grid.Cells[i, j] = chosenIds.First();
                                    grid.NotEmptyCells[i, j] = true;
                                }
                            });
                        });
                        grid.ReservationId.Add(chosenIds.First());
                        break;
                    }
            }
        }


    
    }
}


