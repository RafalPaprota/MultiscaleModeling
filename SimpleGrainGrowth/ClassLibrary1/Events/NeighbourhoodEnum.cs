
using GrainGrowth.Lib.Extensions;
using GrainGrowth.Lib.Models;

using System.Collections.Generic;
using System.Drawing;
using System.Linq;


namespace Grains.Library.Actions
{
    public delegate void NeighbourhoodCalculation(Grid originalMatrix, Cell currentCell, int[,] referenceArray = null, int randomNumber = 1, int x = 100);

    public static class NeighbourhoodActive
    {
        public static NeighbourhoodCalculation MooreActivation => (Grid originalMatrix, Cell currentCell, int[,] referenceArray, int randomNumber, int x) => {

            var coordinates = Unites.Unites.MooreCoordinates;

            NeighbourhoodCalculation(originalMatrix, referenceArray, currentCell, coordinates, 0);
        };

        public static NeighbourhoodCalculation VonNeumannActivation => (Grid originalMatrix, Cell currentCell, int[,] referenceArray, int randomNumber, int x) => {

            var coordinates = Unites.Unites.VonNeumannCoordinates;

            NeighbourhoodCalculation(originalMatrix, referenceArray, currentCell, coordinates, 0);
        };


        public static NeighbourhoodCalculation MooreExtented => (Grid originalMatrix, Cell currentCell, int[,] referenceArray, int randomNumber, int x) =>
        {
            var coordinates = Unites.Unites.MooreCoordinates;

            if (NeighbourhoodCalculation(originalMatrix, referenceArray, currentCell, coordinates, 5))
            {
                return;
            };

            coordinates = Unites.Unites.VonNeumannCoordinates;

            if (NeighbourhoodCalculation(originalMatrix, referenceArray, currentCell, coordinates, 3))
            {
                return;
            };

            coordinates = Unites.Unites.InvertedVonNeumannCoordinates;

            if (NeighbourhoodCalculation(originalMatrix, referenceArray, currentCell, coordinates, 3))
            {
                return;
            };

            if (randomNumber <= x)
            {
                coordinates = Unites.Unites.MooreCoordinates;
                NeighbourhoodCalculation(originalMatrix, referenceArray, currentCell, coordinates, 0);
            }
        };



        static bool NeighbourhoodCalculation(Grid originalMatrix, int[,] referenceArray, Cell currentCell, Point[] coordinates, int treshold)
        {
            var neighbourhoodPoints = new List<int>();

            foreach (var point in coordinates)
            {
                var tempCell = currentCell.Get(point.X, point.Y).ReworkeCell(originalMatrix);
                var tempId = referenceArray[tempCell.X, tempCell.Y];

                if (tempId != 0 && tempId != 1 && !originalMatrix.ReservationId.Contains(tempId))
                {
                    neighbourhoodPoints.Add(referenceArray[tempCell.X, tempCell.Y]);
                }
            }

            if (neighbourhoodPoints.Count() == 0)
            {
                return false;
            }

            var groupedIds = neighbourhoodPoints.GroupBy(i => i).OrderByDescending(grp => grp.Count());
            var mostOftenId = groupedIds.Select(grp => grp.Key).First();
            var occurenceCount = groupedIds.First().Count();

            if (mostOftenId != 0 && occurenceCount >= treshold)
            {
                originalMatrix.Cells[currentCell.X, currentCell.Y] = mostOftenId;
                originalMatrix.NotEmptyCells[currentCell.X, currentCell.Y] = true;
                return true;
            }

            return false;
        }
    }
}
