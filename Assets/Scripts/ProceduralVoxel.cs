using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NoiseTest;

public class ProceduralVoxel
{
    OpenSimplexNoise openSimplex1 = new OpenSimplexNoise(1111);
    //OpenSimplexNoise openSimplex2 = new OpenSimplexNoise(2222);
    //OpenSimplexNoise openSimplex3 = new OpenSimplexNoise(3333);

    // TODO: Bounds of Simplex Noice is [-1, 1]

    public double Sample(Vector3 position)
    {
        return openSimplex1.Evaluate(position.x, position.y, position.z); ;
    }

    public float[,,] generateEmptyCell(int cellSize)
    {
        float[,,] cell = new float[cellSize,cellSize,cellSize];
        for(int x = 0; x < cellSize; x++)
        {
            for (int y = 0; y < cellSize; y++)
            {
                for (int z = 0; z < cellSize; z++)
                {
                    cell[x, y, z] = 0;
                }
            }
        }
        return cell;
    }

    public float[,,] applyNoise(float[,,] cell)
    {
        for (int x = 0; x < cell.Length; x++)
        {
            for (int y = 0; y < cell.Length; y++)
            {
                for (int z = 0; z < cell.Length; z++)
                {
                    cell[x, y, z] = cell[x, y, z] + (float)openSimplex1.Evaluate(x,y,z);
                }
            }
        }
        return cell;
    }
    public float[,,] generateCell(int cellSize)
    {
        float[,,] cell = new float[cellSize, cellSize, cellSize];
        for (int x = 0; x < cellSize; x++)
        {
            for (int y = 0; y < cellSize; y++)
            {
                for (int z = 0; z <cellSize; z++)
                {

                    float fx = x / ((cellSize) - 1.0f);
                    float fy = y / ((cellSize) - 1.0f);
                    float fz = z / ((cellSize) - 1.0f);

                    cell[x, y, z] = (float)openSimplex1.Evaluate(fx, fy, fz);
                }
            }
        }
        return cell;
    }



    // cell orgin of each cell should be at the center of the cell.
    public float[,,] generateCell(int cellSize, Transform orgin)
    {
        float[,,] cell = new float[cellSize, cellSize, cellSize];
        for (int x = 0; x < cellSize; x++)
        {
            for (int y = 0; y < cellSize; y++)
            {
                for (int z = 0; z < cellSize; z++)
                {
                    cell[x, y, z] = (orgin.position.y - (cellSize/2) ) 
                        + (float)openSimplex1.Evaluate(x, y, z);
                }
            }
        }
        return cell;
    }
}