﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretSantaApp.Models
{
  public class CustomUserRepository : ICustomUserRepository
  {
    private readonly AppDbContext _appDbContext;

    public CustomUserRepository(AppDbContext appDbContext)
    {
      _appDbContext = appDbContext;
    }
    public List<CustomUser> AllUsers()
    {
      throw new NotImplementedException();
    }


    public CustomUser CustomUserByUserId(int id)
    {
      var result = new CustomUser();
      result = _appDbContext.CustomUsers.FirstOrDefault(u => u.UserId == id);
      return result;
      
    }

    public CustomUser CustomUserByAccountNumber(string acctno)
    {
      var result = new CustomUser();
      result = _appDbContext.CustomUsers.FirstOrDefault(u => u.AccountNumberString == acctno);
   
      return result;
    }



    public CustomUser SaveUser(CustomUser u)
    {
      var result = new CustomUser();
      result.Email = u.Email;
      result.FullName = u.FullName;
      result.AccountNumberString = u.AccountNumberString;

      _appDbContext.CustomUsers.Add(result);
      _appDbContext.SaveChanges();

      return result;
      //_appDbContext.Groups.Add(result);
      //_appDbContext.SaveChanges();
    }

  }
}