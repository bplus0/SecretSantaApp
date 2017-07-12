﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using SecretSantaApp.DAL;
using SecretSantaApp.Models;
using SecretSantaApp.ViewModels;
using SecretSantaApp.Views.Groups;

namespace SecretSantaApp.BL
{
  public class SecretSantaBl : ISecretSantaBl
  {

    private readonly IGroupDal _groupDal;
    private readonly ICustomUserDal _customUserDal;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IGroupMembershipDal _groupMembershipDal;
    private readonly IGroupRulesDal _groupRulesDal;
    public SecretSantaBl(IGroupDal groupDal,
                         ICustomUserDal customUserDal,
                         IHttpContextAccessor httpContextAccessor,
                          IGroupMembershipDal groupMembershipDal,
                          IGroupRulesDal groupRulesDal)
    {
      _groupDal = groupDal;
      _customUserDal = customUserDal;
      _httpContextAccessor = httpContextAccessor;
      _groupMembershipDal = groupMembershipDal;
      _groupRulesDal = groupRulesDal;
    }

    public CustomUserEditModel CustomUserModelByLoggedInUser(ClaimsPrincipal user)
    {
      //var test = HttpContext.User.Identity.GetUserId();
      var result = new CustomUserEditModel();
      var acctid = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
      var name = user.Identity.Name;
      var email = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;

      result.AccountNumberString = acctid;
      result.FullName = name;
      result.Email = email;

      return result;
    }



    public string UserFullNameByAccountNumberString(string acctno)
    {
      var user = _customUserDal.CustomUserByAccountNumber(acctno);

      var result = user.FullName;

      return result;
    }

    public GroupAdminModel DefaultGroupAdminModel()
    {
      var result = new GroupAdminModel();
      result.ActiveGroups = _groupDal.AllActiveGroups();

      return result;
    }


    public GroupEditModel DefaultGroupEditModel()
    {
      var result = new GroupEditModel();
      return result;
    }

    public GroupEditModel SaveNewGroup(GroupEditModel model)
    {
      //Check to make sure there are values
      if (model.GroupName == null)
      {
        throw new Exception("Name is Required");
      }


      //Save the new group
      var saved = _groupDal.SaveNewGroup(model);
      model.Update(saved);

      //Now we will add the user who is creating the group to the new group
      var gmd = new GroupMembershipEditModel();
      gmd.AccountNumberString = "";
      gmd.GroupId = saved.GroupId;

      _groupMembershipDal.SaveMemberToGroup(gmd);

      model.Saved = true;
      return model;
    }


    public CustomUserEditModel CheckUserByCustomUserAccountNumber(CustomUserEditModel model)
    {
      //First check to see if this user exists
      //_customUserDal.SaveUser(model);
      var exists = _customUserDal.CustomUserByAccountNumber(model.AccountNumberString);

      if (exists != null)
      {
        model.UserId = exists.UserId;
        return model;
      }
      else
      {
        //Create a new user
        var newuser = _customUserDal.SaveUser(model);
        model.UserId = newuser.UserId;
      }

      return model;
      //var result = new CustomUserEditModel();
      //return result;
    }


    public void JoinGroupAsCustomUser(CustomUserEditModel user, int groupid)
    {

      var gmd = new GroupMembershipEditModel();
      gmd.AccountNumberString = user.AccountNumberString;
      gmd.GroupId = groupid;

      _groupMembershipDal.SaveMemberToGroup(gmd);

    }


    public MyGroupsViewModel MyGroupsViewModelByUserId(CustomUserEditModel user)
    {
      var result = new MyGroupsViewModel();
      result.MyGroups = new List<Group>();
      var grouplist = new List<Group>();

      var groupsibelongto = _groupMembershipDal.GroupsBelongingToUserAccountNumberString(user.AccountNumberString);

      foreach (var g in groupsibelongto)
      {
        var group = new Group();
        group = _groupDal.GetGroupById(g.GroupId);
        grouplist.Add(group);
      }

      result.MyGroups = grouplist;

      return result;
    }


    public GroupHomeEditModel GroupHomeEditModelByGroupId(int groupid)
    {
      var result = new GroupHomeEditModel();
      var userlist = new List<CustomUserEditModel>();
      var loggedinuser = _httpContextAccessor.HttpContext.Session.GetObjectFromJson<CustomUserEditModel>("LoggedInUser");

      var group = _groupDal.GetGroupById(groupid);


      var admin = _customUserDal.CustomUserByAccountNumber(group.InsertedBy);

      result.GroupAdmin = admin;
      //if the user is the person who created the group assign them to admin.
      result.GroupAdminBool = @group.InsertedBy == loggedinuser.AccountNumberString;

      result.Update(group);

      var groupmembership = _groupMembershipDal.AllGroupMembersByGroupId(groupid);

      foreach (var g in groupmembership)
      {
        var user = new CustomUser();
        var cu = new CustomUserEditModel();
        user = _customUserDal.CustomUserByAccountNumber(g.AccountNumberString);
        cu.Update(user);
        userlist.Add(cu);
      }

      result.GroupMembers = userlist;
      result.NewGroup = false;


      return result;
    }




    public JoinGroupEditModel JoinGroupEditModelByAccountNumberString(string acctno)
    {
      var result = new JoinGroupEditModel();
      var groupslist = new List<Group>();
      var user = _customUserDal.CustomUserByAccountNumber(acctno);

      var activegroupsidlist = new List<int>();
      var groupsmemberofidlist = new List<int>();

      var allactivegroups = _groupDal.AllActiveGroups();
      var groupsmemberof = _groupMembershipDal.GroupsBelongingToUserAccountNumberString(user.AccountNumberString);


      foreach (var ag in allactivegroups)
      {
        activegroupsidlist.Add(ag.GroupId);
      }

      foreach (var ag in groupsmemberof)
      {
        groupsmemberofidlist.Add(ag.GroupId);
      }


      var results = activegroupsidlist.Where(m => !groupsmemberofidlist.Contains(m));

      foreach (var r in results)
      {
        var group = new Group();
        group = _groupDal.GetGroupById(r);
        groupslist.Add(group);
      }

      //var matchItem = List1.Intersect(List2).First();



      //foreach (var g in groupsmemberof)
      //{
      //  var group = new Group();
      //  group = _groupDal.GetGroupById(g.GroupId);
      //  groupslist.Add(group);
      //}

      result.CustomUser = new CustomUserEditModel();
      result.CustomUser.Update(user);

      result.GroupsNotMemberOf = groupslist;

      return result;
    }



    public InviteUsersCollectionModel InviteUsersCollectionModelByAmountToGet(int amount)
    {

      var result = new InviteUsersCollectionModel();
      result.UsersToInvite = new List<InviteUsersViewModel>();

      if (amount <= 0)
      {
        return result;
      }
      else
      {
        for (var a = 0; a < amount; a++)
        {
          var usertoinvite = new InviteUsersViewModel();
          result.UsersToInvite.Add(usertoinvite);
        }
      }
      return result;
    }


    public InviteUsersViewModel AdditionalInviteUsersViewModel(int tempid)
    {
      var result = new InviteUsersViewModel();
      result.TempId = tempid;
      return result;
    }


    public JoinGroupEditModel JoinGroupEditModelByGroupId(int groupid)
    {
      var group = _groupDal.GetGroupById(groupid);

      if (group == null)
      {
        throw new Exception("Error loading group");
      }



      var result = new JoinGroupEditModel();
      result.Group = group;
      result.GroupId = groupid;
      result.Verified = false;

      return result;

    }


    public JoinGroupEditModel CheckPasswordInput(JoinGroupEditModel model)
    {
      var group = _groupDal.GetGroupById(model.GroupId);

      if (group == null)
      {
        throw new Exception("Error loading group");
      }

      if (model.UserInputGroupPassword == null)
      {
        throw new Exception("Password is required");
      }

      var password = group.GroupPassWord;

      if (model.UserInputGroupPassword != password)
      {
        //throw new Exception("Incorrect password");
        model.Verified = false;
        model.ErrorMsg = "Incorrect Password";
        model.Group = group;
        model.GroupId = group.GroupId;
        return model;
      }
      else
      {
        model.Group = group;
        model.GroupId = group.GroupId;

        JoinGroupAsCustomUser(model.CustomUser, model.GroupId);
        model.Verified = true;
        model.ErrorMsg = null;
        return model;
      }

    }





    public GroupRulesEditModel NewRuleEditModelByGroupId(int groupid)
    {
      var group = _groupDal.GetGroupById(groupid);

      var result = new GroupRulesEditModel();
      result.GroupName = group.GroupName;
      result.GroupId = groupid;


      return result;
    }



    public GroupRulesEditModel GroupRuleEditModelByRuleId(int ruleid)
    {
      var rule = _groupRulesDal.GetRuleByRuleId(ruleid);
      var group = _groupDal.GetGroupById(rule.GroupId);
      var result = new GroupRulesEditModel();
      result.Update(rule);
      result.ID = ruleid;
      result.GroupName = group.GroupName;

      return result;

    }
    
    
    public GroupRulesEditModel SaveGroupRules(GroupRulesEditModel model)
    {
      if (model.Rule == null)
      {
        throw new Exception("Required");
      }

      var saved = _groupRulesDal.SaveRules(model);
      
      var result = new GroupRulesEditModel();
      result.Update(saved);

      return result;
    }


    public GroupRulesDisplayModel GroupRulesDisplayModelByGroupId(int groupid)
    {
      var group = _groupDal.GetGroupById(groupid);
      var grouprules = _groupRulesDal.RulesByGroupId(groupid);

      var result = new GroupRulesDisplayModel();
      result.GroupName = group.GroupName;
      result.GroupRules = grouprules;
      return result;
    }




  }
}
