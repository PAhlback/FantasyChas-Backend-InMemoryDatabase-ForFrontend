﻿namespace FantasyChas_Backend.Models.ViewModels
{
    public class CharacterViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Age { get; set; }
        public string? Gender { get; set; } /* vad har vi kontrollen?*/
        public int Level { get; set; }
        public int HealthPoints { get; set; }
        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public int Intelligence { get; set; }
        public int Wisdom { get; set; }
        public int Constitution { get; set; }
        public int Charisma { get; set; }
        public string? Backstory { get; set; }
        public string? ProfessionName { get; set; }
        public string? SpeciesName { get; set; }
    }
}
