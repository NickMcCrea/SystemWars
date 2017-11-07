using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSRestMatch.GameServer
{
    public class Weapon
    {
        public string Name { get; set; }
        public int Damage { get; set; }
        public int FiringInterval { get; set; }
        public float ProjectileSpeed { get; set; }
        public float DamageRadius { get; set; }
    }

    public static class WeaponFactory
    {
        public static Weapon CreatePistol()
        {
            return new Weapon() { Name = "Pistol", Damage = 5, DamageRadius = 0.1f, FiringInterval = 1000, ProjectileSpeed = 0.01f };
        }
    }
}
