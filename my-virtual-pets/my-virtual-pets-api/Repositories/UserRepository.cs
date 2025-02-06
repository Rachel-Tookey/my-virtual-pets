﻿using Microsoft.EntityFrameworkCore;
using my_virtual_pets_api.Data;
using my_virtual_pets_api.Entities;
using my_virtual_pets_api.Repositories.Interfaces;
using my_virtual_pets_class_library.DTO;

namespace my_virtual_pets_api.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbContext _context;

        public UserRepository(IDbContext context)
        {
            _context = context;
        }

        public Guid GetUserIdByUsername(string username)
        {
            return _context.GlobalUsers.FirstOrDefault(u => u.Username == username).Id;
        }

        public bool ExistsByUsername(string username)
        {
            return _context.GlobalUsers.Any(u => u.Username == username);
        }

        public bool ExistsByEmail(string email)
        {
            return _context.GlobalUsers.Any(u => u.Email == email);
        }

        public Guid CreateNewGlobalUser(NewUserDTO newUserDto)
        {
            GlobalUser newGlobalUser = new GlobalUser(newUserDto);
            _context.GlobalUsers.Add(newGlobalUser);
            _context.SaveChanges();
            return newGlobalUser.Id;
        }

        public Guid CreateNewLocalUser(NewUserDTO newUserDto, Guid globalUserId)
        {
            LocalUser newLocalUser = new LocalUser(newUserDto, globalUserId);
            _context.LocalUsers.Add(newLocalUser);
            _context.SaveChanges();
            return newLocalUser.Id;
        }

        public Guid CreateNewAuthUser(NewUserDTO newUserDto, Guid globalUserId)
        {
            throw new NotImplementedException();
        }

        public string GetPassword(string username)
        {
            var userGuid = _context.GlobalUsers.FirstOrDefault(u => u.Username == username).Id;
            var userPassword = _context.LocalUsers.FirstOrDefault(u => u.GlobalUserId == userGuid).Password;
            return userPassword;
        }

        public UserDisplayDTO GetUserDetailsByUserId(Guid userId)
        {
            var user = _context.GlobalUsers
                .Where(u => u.Id == userId)
                .FirstOrDefault();

            var localUser = _context.LocalUsers
                .Where(lu => lu.GlobalUserId == userId)
                .FirstOrDefault();

            if (user == null || localUser == null)
            {
                return null;
            }

            var petCount = _context.Pets
                .Where(p => p.GlobalUserId == userId)
                .Count();

            var userDisplayDTO = new UserDisplayDTO
            {
                Username = user.Username,
                FirstName = localUser.FirstName,
                LastName = localUser.LastName,
                Email = user.Email,
                PetCount = petCount
            };

            return userDisplayDTO;
        }

        public bool AddToFavourites(Guid GlobalUserId, Guid PetId)
        {
            //TO DO: better error handling
            var user = _context.GlobalUsers.SingleOrDefault(g => g.Id == GlobalUserId);
            if (user == null) throw new KeyNotFoundException("GlobalUser does not exist");

            var pet = _context.Pets.SingleOrDefault(p => p.Id == PetId);
            if (pet == null) throw new KeyNotFoundException("Pet not exist");

            var existingFavourite = _context.Favorites.SingleOrDefault(f => f.GlobalUserId == GlobalUserId && f.PetId == PetId);
            if (existingFavourite != null) return false;

            var favourite = new Favourite { GlobalUserId = GlobalUserId, PetId = PetId };
            _context.Favorites.Add(favourite);
            _context.SaveChanges();
            return true;
        }

        public List<Guid> GetFavoritePetIds(Guid GlobalUserId)
        {
            var user = _context.GlobalUsers.Include(g => g.Favourites).SingleOrDefault(g => g.Id == GlobalUserId);
            if (user == null) throw new KeyNotFoundException("GlobalUser does not exist");
            var favouritePetIds = user.Favourites.Select(f => f.PetId).ToList();
            return favouritePetIds;
        }

        public List<PetCardDataDTO> GetFavoritePetCards(Guid GlobalUserId)
        {
            var user = _context.GlobalUsers.Include(g => g.Favourites)
                                            .ThenInclude(f => f.Pet)
                                            .ThenInclude(p => p.Image)
                                            .SingleOrDefault(g => g.Id == GlobalUserId);
            if (user == null) throw new KeyNotFoundException("GlobalUser does not exist");
            var favouritePets = user.Favourites.Select(f => f.Pet).ToList();
            if (favouritePets.Count == 0) return [];
            return favouritePets.Select(p => Pet.CreatePetCardDto(p)).ToList();

        }

        public bool RemoveFromFavourites(Guid GlobalUserId, Guid PetId)
        {
            var user = _context.GlobalUsers.Include(g => g.Favourites);
            if (user == null) throw new KeyNotFoundException("GlobalUser does not exist");

            var pet = _context.Pets.SingleOrDefault(p => p.Id == PetId);
            if (pet == null) throw new KeyNotFoundException("Pet does not exist");

            var existingFavourite = _context.Favorites.SingleOrDefault(f => f.GlobalUserId == GlobalUserId && f.PetId == PetId);
            if (existingFavourite == null) return false;

            _context.Favorites.Remove(existingFavourite);
            _context.SaveChanges();
            return true;
        }
    }
}
