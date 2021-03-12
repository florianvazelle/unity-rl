using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sokoban {

    // Sokoban Level
    public struct Level {   

        // All the grid possibility
        public enum Grid { EMPTY = 0, WALL = 1, BOX = 2, GOAL = 3, PLAYER = 4 };

        // All this informations are the level representation at the start of the game
        public readonly Vector2 player;                         // Player position
        public readonly List<Vector2> walls, boxes, goals;      // All the elements of the level
        
        public readonly int WIDTH, HEIGHT;                      // Width and heigth of the level to parse grid
        public readonly List<int> grid;                         // Sokoban grid level representation

        public Level(List<int> grid, int width, int height) {
            this.grid = new List<int>(grid);
            this.WIDTH = width;
            this.HEIGHT = height;

            this.player = new Vector2(0, 0);
            this.walls = new List<Vector2>();
            this.boxes = new List<Vector2>();
            this.goals = new List<Vector2>();

            for(int y = 0; y < height; y++) {
                for(int x = 0; x < width; x++) {
                    Vector2 pos = new Vector2(x, y);

                    int value = this.grid[x * HEIGHT + y];
                    if (value == (int) Grid.WALL) this.walls.Add(pos);
                    if (value == (int) Grid.BOX) this.boxes.Add(pos);
                    if (value == (int) Grid.GOAL) this.goals.Add(pos);
                    if (value == (int) Grid.PLAYER) this.player = pos;
                }
            }
        }
    }
}