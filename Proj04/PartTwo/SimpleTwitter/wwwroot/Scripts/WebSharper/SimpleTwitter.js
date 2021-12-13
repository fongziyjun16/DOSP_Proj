(function(Global)
{
 "use strict";
 var SimpleTwitter,UsernameToken,FollowInfo,TweetInfo,FollowingNewTweetInfo,AccountPageProcess,MainPageProcess,SimpleTwitter_Templates,SimpleTwitter_JsonDecoder,SimpleTwitter_JsonEncoder,WebSharper,Concurrency,Remoting,AjaxRemotingProvider,JSON,UI,Var$1,Templating,Runtime,Server,ProviderBuilder,Handler,TemplateInstance,Operators,console,Arrays,JavaScript,Promise,AspNetCore,WebSocket,Client,WithEncoding,Unchecked,Client$1,Templates,ClientSideJson,Provider;
 SimpleTwitter=Global.SimpleTwitter=Global.SimpleTwitter||{};
 UsernameToken=SimpleTwitter.UsernameToken=SimpleTwitter.UsernameToken||{};
 FollowInfo=SimpleTwitter.FollowInfo=SimpleTwitter.FollowInfo||{};
 TweetInfo=SimpleTwitter.TweetInfo=SimpleTwitter.TweetInfo||{};
 FollowingNewTweetInfo=SimpleTwitter.FollowingNewTweetInfo=SimpleTwitter.FollowingNewTweetInfo||{};
 AccountPageProcess=SimpleTwitter.AccountPageProcess=SimpleTwitter.AccountPageProcess||{};
 MainPageProcess=SimpleTwitter.MainPageProcess=SimpleTwitter.MainPageProcess||{};
 SimpleTwitter_Templates=Global.SimpleTwitter_Templates=Global.SimpleTwitter_Templates||{};
 SimpleTwitter_JsonDecoder=Global.SimpleTwitter_JsonDecoder=Global.SimpleTwitter_JsonDecoder||{};
 SimpleTwitter_JsonEncoder=Global.SimpleTwitter_JsonEncoder=Global.SimpleTwitter_JsonEncoder||{};
 WebSharper=Global.WebSharper;
 Concurrency=WebSharper&&WebSharper.Concurrency;
 Remoting=WebSharper&&WebSharper.Remoting;
 AjaxRemotingProvider=Remoting&&Remoting.AjaxRemotingProvider;
 JSON=Global.JSON;
 UI=WebSharper&&WebSharper.UI;
 Var$1=UI&&UI.Var$1;
 Templating=UI&&UI.Templating;
 Runtime=Templating&&Templating.Runtime;
 Server=Runtime&&Runtime.Server;
 ProviderBuilder=Server&&Server.ProviderBuilder;
 Handler=Server&&Server.Handler;
 TemplateInstance=Server&&Server.TemplateInstance;
 Operators=WebSharper&&WebSharper.Operators;
 console=Global.console;
 Arrays=WebSharper&&WebSharper.Arrays;
 JavaScript=WebSharper&&WebSharper.JavaScript;
 Promise=JavaScript&&JavaScript.Promise;
 AspNetCore=WebSharper&&WebSharper.AspNetCore;
 WebSocket=AspNetCore&&AspNetCore.WebSocket;
 Client=WebSocket&&WebSocket.Client;
 WithEncoding=Client&&Client.WithEncoding;
 Unchecked=WebSharper&&WebSharper.Unchecked;
 Client$1=UI&&UI.Client;
 Templates=Client$1&&Client$1.Templates;
 ClientSideJson=WebSharper&&WebSharper.ClientSideJson;
 Provider=ClientSideJson&&ClientSideJson.Provider;
 UsernameToken.New=function(username,token)
 {
  return{
   username:username,
   token:token
  };
 };
 FollowInfo.New=function(username,follower)
 {
  return{
   username:username,
   follower:follower
  };
 };
 TweetInfo.New=function(creator,content,retweetID)
 {
  return{
   creator:creator,
   content:content,
   retweetID:retweetID
  };
 };
 FollowingNewTweetInfo.New=function(id,creator,content,retweetID)
 {
  return{
   id:id,
   creator:creator,
   content:content,
   retweetID:retweetID
  };
 };
 AccountPageProcess.formProcess$35$20=function(state)
 {
  return function(e)
  {
   var b;
   Concurrency.StartImmediate((b=null,Concurrency.Delay(function()
   {
    var username,password;
    username=e.Vars.Hole("username").$1.Get();
    password=e.Vars.Hole("password").$1.Get();
    return username.length>0&&password.length>0?(new AjaxRemotingProvider.New()).Sync("SimpleTwitter:SimpleTwitter.RPCServer.SignUpProcess:1209569782",[username,password])?(state.Set("Sign Up Success"),Concurrency.Zero()):(state.Set("Username has EXISTED"),Concurrency.Zero()):(state.Set("Username & Password are REQUIRED"),Concurrency.Zero());
   })),null);
  };
 };
 AccountPageProcess.formProcess$13$20=function(state)
 {
  return function(e)
  {
   var b;
   Concurrency.StartImmediate((b=null,Concurrency.Delay(function()
   {
    var username,password;
    username=e.Vars.Hole("username").$1.Get();
    password=e.Vars.Hole("password").$1.Get();
    return username.length>0&&password.length>0?Concurrency.Bind((new AjaxRemotingProvider.New()).Async("SimpleTwitter:SimpleTwitter.RPCServer.SignInProcess:770625658",[username,password]),function(a)
    {
     var verificationObj;
     verificationObj=JSON.parse(a);
     return Global.String(new Global.Boolean(verificationObj.loginSign))=="true"?(self.document.cookie="username="+username,self.document.cookie="token="+Global.String(verificationObj.token),state.Set(username+" sign in"),self.location.href=self.location.href+"main",Concurrency.Zero()):(state.Set("Incorrect Username or Password"),Concurrency.Zero());
    }):(state.Set("Username & Password are REQUIRED"),Concurrency.Zero());
   })),null);
  };
 };
 AccountPageProcess.formProcess=function()
 {
  var state,b,o,_this,t,t$1,p,i;
  state=Var$1.Create$1("");
  return(b=(o=state.get_View(),(_this=(t=(t$1=new ProviderBuilder.New$1(),(t$1.h.push(Handler.EventQ2(t$1.k,"signin",function()
  {
   return t$1.i;
  },function(e)
  {
   var b$1;
   Concurrency.StartImmediate((b$1=null,Concurrency.Delay(function()
   {
    var username,password;
    username=e.Vars.Hole("username").$1.Get();
    password=e.Vars.Hole("password").$1.Get();
    return username.length>0&&password.length>0?Concurrency.Bind((new AjaxRemotingProvider.New()).Async("SimpleTwitter:SimpleTwitter.RPCServer.SignInProcess:770625658",[username,password]),function(a)
    {
     var verificationObj;
     verificationObj=JSON.parse(a);
     return Global.String(new Global.Boolean(verificationObj.loginSign))=="true"?(self.document.cookie="username="+username,self.document.cookie="token="+Global.String(verificationObj.token),state.Set(username+" sign in"),self.location.href=self.location.href+"main",Concurrency.Zero()):(state.Set("Incorrect Username or Password"),Concurrency.Zero());
    }):(state.Set("Username & Password are REQUIRED"),Concurrency.Zero());
   })),null);
  })),t$1)),(t.h.push(Handler.EventQ2(t.k,"signup",function()
  {
   return t.i;
  },function(e)
  {
   var b$1;
   Concurrency.StartImmediate((b$1=null,Concurrency.Delay(function()
   {
    var username,password;
    username=e.Vars.Hole("username").$1.Get();
    password=e.Vars.Hole("password").$1.Get();
    return username.length>0&&password.length>0?(new AjaxRemotingProvider.New()).Sync("SimpleTwitter:SimpleTwitter.RPCServer.SignUpProcess:1209569782",[username,password])?(state.Set("Sign Up Success"),Concurrency.Zero()):(state.Set("Username has EXISTED"),Concurrency.Zero()):(state.Set("Username & Password are REQUIRED"),Concurrency.Zero());
   })),null);
  })),t)),(_this.h.push({
   $:2,
   $0:"operationstate",
   $1:o
  }),_this))),(p=Handler.CompleteHoles(b.k,b.h,[["username",0],["password",0]]),(i=new TemplateInstance.New(p[1],SimpleTwitter_Templates.accountform(p[0])),b.i=i,i))).get_Doc();
 };
 MainPageProcess.Process$184$33=function(getCookie,realTimeServer)
 {
  return function()
  {
   var b;
   Concurrency.StartImmediate((b=null,Concurrency.Delay(function()
   {
    var username;
    username=getCookie("username");
    realTimeServer.$0.Post({
     $:6,
     $0:username
    });
    return Concurrency.Zero();
   })),null);
  };
 };
 MainPageProcess.Process$176$33=function(realTimeServer)
 {
  return function(e)
  {
   var b;
   Concurrency.StartImmediate((b=null,Concurrency.Delay(function()
   {
    var hashtag;
    hashtag=new Global.String(e.Vars.Hole("hahstagname").$1.Get());
    return hashtag.length>0?(realTimeServer.$0.Post({
     $:5,
     $0:Global.String(hashtag)
    }),Concurrency.Zero()):Concurrency.Zero();
   })),null);
  };
 };
 MainPageProcess.Process$168$35=function(realTimeServer)
 {
  return function(e)
  {
   var b;
   Concurrency.StartImmediate((b=null,Concurrency.Delay(function()
   {
    var followingName;
    followingName=new Global.String(e.Vars.Hole("followingname").$1.Get());
    return followingName.length>0?(realTimeServer.$0.Post({
     $:4,
     $0:Global.String(followingName)
    }),Concurrency.Zero()):Concurrency.Zero();
   })),null);
  };
 };
 MainPageProcess.Process$155$21=function(getCookie,realTimeServer)
 {
  return function(e)
  {
   var b;
   Concurrency.StartImmediate((b=null,Concurrency.Delay(function()
   {
    var content;
    content=new Global.String(e.Vars.Hole("retweetcontent").$1.Get());
    return content.length>0?(realTimeServer.$0.Post({
     $:3,
     $0:JSON.stringify(TweetInfo.New(getCookie("username"),Global.String(content),Operators.toInt(e.Vars.Hole("retweetid").$1.Get())))
    }),Concurrency.Zero()):Concurrency.Zero();
   })),null);
  };
 };
 MainPageProcess.Process$142$23=function(getCookie,realTimeServer)
 {
  return function(e)
  {
   var b;
   Concurrency.StartImmediate((b=null,Concurrency.Delay(function()
   {
    var content;
    content=new Global.String(e.Vars.Hole("tweetcontent").$1.Get());
    return content.length>0?(realTimeServer.$0.Post({
     $:3,
     $0:JSON.stringify(TweetInfo.New(getCookie("username"),Global.String(content),-1))
    }),Concurrency.Zero()):Concurrency.Zero();
   })),null);
  };
 };
 MainPageProcess.Process$123$22=function(loginVerify,getCookie,realTimeServer,setElementInnerHTML)
 {
  return function(e)
  {
   var b;
   Concurrency.StartImmediate((b=null,Concurrency.Delay(function()
   {
    var followingUsername;
    loginVerify();
    followingUsername=e.Vars.Hole("following").$1.Get();
    return followingUsername.length>0?(realTimeServer.$0.Post({
     $:2,
     $0:JSON.stringify(FollowInfo.New(followingUsername,getCookie("username")))
    }),e.Vars.Hole("following").$1.Set(""),Concurrency.Zero()):(setElementInnerHTML(["followState","Follow Input has been REQUIRED"]),Concurrency.Zero());
   })),null);
  };
 };
 MainPageProcess.Process$110$20=function(logoutOperation,getCookie,realTimeServer)
 {
  return function()
  {
   var b;
   Concurrency.StartImmediate((b=null,Concurrency.Delay(function()
   {
    var usernameToken;
    logoutOperation();
    usernameToken=UsernameToken.New(getCookie("username"),getCookie("token"));
    console.log(realTimeServer.$0);
    realTimeServer.$0.Post({
     $:1,
     $0:JSON.stringify(usernameToken)
    });
    return Concurrency.Zero();
   })),null);
  };
 };
 MainPageProcess.Process=function(ep)
 {
  var realTimeServer,b,username,b$1,t,t$1,t$2,t$3,t$4,t$5,t$6,U,_this,p,i;
  function getCookie(key)
  {
   var cookies;
   cookies=(new Global.String(self.document.cookie)).split(";");
   return key==="username"?Arrays.get(cookies,0).substring(Arrays.get(cookies,0).indexOf("=")+1):key==="token"?Arrays.get(cookies,1).substring(Arrays.get(cookies,1).indexOf("=")+1):"";
  }
  function logoutOperation()
  {
   var accountURL;
   self.document.cookie="username=";
   self.document.cookie="token=";
   accountURL=new Global.String(self.location.href);
   accountURL=new Global.String(accountURL.substring(0,accountURL.lastIndexOf("/")));
   self.location.replace(Global.String(accountURL));
  }
  function loginVerify()
  {
   realTimeServer.$0.Post({
    $:0,
    $0:JSON.stringify(UsernameToken.New(getCookie("username"),getCookie("token")))
   });
  }
  function setElementInnerHTML(eleID,content)
  {
   self.document.getElementById(eleID).innerHTML=content;
  }
  function addLi2Table(tableID,tweetInfo,clearFirst)
  {
   var ul,li;
   ul=self.document.getElementById(tableID);
   if(clearFirst)
    ul.innerHTML="";
   li=self.document.createElement("li");
   li.appendChild(self.document.createTextNode(tweetInfo));
   li.setAttribute("class","list-group-item");
   ul.appendChild(li);
  }
  function addTweets2Table(qryTweets,tableID)
  {
   var tweets,i$1,$1;
   addLi2Table(tableID,"",true);
   tweets=(new Global.String(qryTweets)).split("}{");
   console.log(tweets);
   for(i$1=0,$1=tweets.length-1;i$1<=$1;i$1++)addLi2Table(tableID,Arrays.get(tweets,i$1),false);
  }
  realTimeServer=null;
  Promise.OfAsync((b=null,Concurrency.Delay(function()
  {
   return WithEncoding.ConnectStateful(function(a)
   {
    return JSON.stringify((SimpleTwitter_JsonEncoder.j())(a));
   },function(a)
   {
    return(SimpleTwitter_JsonDecoder.j())(JSON.parse(a));
   },ep,function()
   {
    var b$2;
    b$2=null;
    return Concurrency.Delay(function()
    {
     return Concurrency.Return([0,function(state)
     {
      return function(msg)
      {
       var b$3;
       b$3=null;
       return Concurrency.Delay(function()
       {
        var data;
        return msg.$==0?(data=msg.$0,Concurrency.Combine(data.$==1?Unchecked.Equals(data.$0,false)?(setElementInnerHTML("followState","Follow Fail. Following not exist or has been followed"),Concurrency.Zero()):(setElementInnerHTML("followState","Follow Success"),Concurrency.Zero()):data.$==2?(addLi2Table("realTimeTweetTable",data.$0,false),Concurrency.Zero()):data.$==3?(addTweets2Table(data.$0,"followingTweetTable"),Concurrency.Zero()):data.$==4?(addTweets2Table(data.$0,"qryHashtagTweetTable"),Concurrency.Zero()):data.$==5?(addTweets2Table(data.$0,"qryMentionTable"),Concurrency.Zero()):Unchecked.Equals(data.$0,false)?(logoutOperation(),Concurrency.Zero()):Concurrency.Zero(),Concurrency.Delay(function()
        {
         return Concurrency.Return(state+1);
        }))):msg.$==3?(console.log("websocket close"),logoutOperation(),Concurrency.Return(state)):msg.$==1?(logoutOperation(),Concurrency.Return(state)):(console.log("websocket open"),Concurrency.Return(state));
       });
      };
     }]);
    });
   });
  }))).then(function(x)
  {
   realTimeServer={
    $:1,
    $0:x
   };
  });
  username=Var$1.Create$1("");
  username.Set(getCookie("username"));
  return(b$1=(t=(t$1=(t$2=(t$3=(t$4=(t$5=(t$6=(U=username.get_View(),(_this=new ProviderBuilder.New$1(),(_this.h.push({
   $:2,
   $0:"username",
   $1:U
  }),_this))),(t$6.h.push(Handler.EventQ2(t$6.k,"logout",function()
  {
   return t$6.i;
  },function()
  {
   var b$2;
   Concurrency.StartImmediate((b$2=null,Concurrency.Delay(function()
   {
    var usernameToken;
    logoutOperation();
    usernameToken=UsernameToken.New(getCookie("username"),getCookie("token"));
    console.log(realTimeServer.$0);
    realTimeServer.$0.Post({
     $:1,
     $0:JSON.stringify(usernameToken)
    });
    return Concurrency.Zero();
   })),null);
  })),t$6)),(t$5.h.push(Handler.EventQ2(t$5.k,"tofollow",function()
  {
   return t$5.i;
  },function(e)
  {
   var b$2;
   Concurrency.StartImmediate((b$2=null,Concurrency.Delay(function()
   {
    var followingUsername;
    loginVerify();
    followingUsername=e.Vars.Hole("following").$1.Get();
    return followingUsername.length>0?(realTimeServer.$0.Post({
     $:2,
     $0:JSON.stringify(FollowInfo.New(followingUsername,getCookie("username")))
    }),e.Vars.Hole("following").$1.Set(""),Concurrency.Zero()):(setElementInnerHTML("followState","Follow Input has been REQUIRED"),Concurrency.Zero());
   })),null);
  })),t$5)),(t$4.h.push(Handler.EventQ2(t$4.k,"posttweet",function()
  {
   return t$4.i;
  },function(e)
  {
   var b$2;
   Concurrency.StartImmediate((b$2=null,Concurrency.Delay(function()
   {
    var content;
    content=new Global.String(e.Vars.Hole("tweetcontent").$1.Get());
    return content.length>0?(realTimeServer.$0.Post({
     $:3,
     $0:JSON.stringify(TweetInfo.New(getCookie("username"),Global.String(content),-1))
    }),Concurrency.Zero()):Concurrency.Zero();
   })),null);
  })),t$4)),(t$3.h.push(Handler.EventQ2(t$3.k,"retweet",function()
  {
   return t$3.i;
  },function(e)
  {
   var b$2;
   Concurrency.StartImmediate((b$2=null,Concurrency.Delay(function()
   {
    var content;
    content=new Global.String(e.Vars.Hole("retweetcontent").$1.Get());
    return content.length>0?(realTimeServer.$0.Post({
     $:3,
     $0:JSON.stringify(TweetInfo.New(getCookie("username"),Global.String(content),Operators.toInt(e.Vars.Hole("retweetid").$1.Get())))
    }),Concurrency.Zero()):Concurrency.Zero();
   })),null);
  })),t$3)),(t$2.h.push(Handler.EventQ2(t$2.k,"qrytweetfromfollowing",function()
  {
   return t$2.i;
  },function(e)
  {
   var b$2;
   Concurrency.StartImmediate((b$2=null,Concurrency.Delay(function()
   {
    var followingName;
    followingName=new Global.String(e.Vars.Hole("followingname").$1.Get());
    return followingName.length>0?(realTimeServer.$0.Post({
     $:4,
     $0:Global.String(followingName)
    }),Concurrency.Zero()):Concurrency.Zero();
   })),null);
  })),t$2)),(t$1.h.push(Handler.EventQ2(t$1.k,"qrytweetfromhashtag",function()
  {
   return t$1.i;
  },function(e)
  {
   var b$2;
   Concurrency.StartImmediate((b$2=null,Concurrency.Delay(function()
   {
    var hashtag;
    hashtag=new Global.String(e.Vars.Hole("hahstagname").$1.Get());
    return hashtag.length>0?(realTimeServer.$0.Post({
     $:5,
     $0:Global.String(hashtag)
    }),Concurrency.Zero()):Concurrency.Zero();
   })),null);
  })),t$1)),(t.h.push(Handler.EventQ2(t.k,"qrytweetfrommention",function()
  {
   return t.i;
  },function()
  {
   var b$2;
   Concurrency.StartImmediate((b$2=null,Concurrency.Delay(function()
   {
    var username$1;
    username$1=getCookie("username");
    realTimeServer.$0.Post({
     $:6,
     $0:username$1
    });
    return Concurrency.Zero();
   })),null);
  })),t)),(p=Handler.CompleteHoles(b$1.k,b$1.h,[["following",0],["tweetcontent",0],["retweetid",1],["retweetcontent",0],["followingname",0],["hahstagname",0]]),(i=new TemplateInstance.New(p[1],SimpleTwitter_Templates.main(p[0])),b$1.i=i,i))).get_Doc();
 };
 SimpleTwitter_Templates.accountform=function(h)
 {
  Templates.LoadLocalTemplates("account");
  return h?Templates.NamedTemplate("account",{
   $:1,
   $0:"accountform"
  },h):void 0;
 };
 SimpleTwitter_JsonDecoder.j=function()
 {
  return SimpleTwitter_JsonDecoder._v?SimpleTwitter_JsonDecoder._v:SimpleTwitter_JsonDecoder._v=(Provider.DecodeUnion(void 0,{
   mentionTweet:5,
   hashtagTweet:4,
   followingTweet:3,
   followingNewTweet:2,
   followResult:1,
   value:0
  },[["LoginVerifyResult",[["$0","value",Provider.Id(),0]]],["FollowResult",[["$0","followResult",Provider.Id(),0]]],["FollowingNewTweet",[["$0","followingNewTweet",Provider.Id(),0]]],["QryFollowingNameTweet",[["$0","followingTweet",Provider.Id(),0]]],["QryHashtagTweet",[["$0","hashtagTweet",Provider.Id(),0]]],["QryMentionTweet",[["$0","mentionTweet",Provider.Id(),0]]]]))();
 };
 SimpleTwitter_JsonEncoder.j=function()
 {
  return SimpleTwitter_JsonEncoder._v?SimpleTwitter_JsonEncoder._v:SimpleTwitter_JsonEncoder._v=(Provider.EncodeUnion(void 0,{
   mention:6,
   hashtag:5,
   name:4,
   content:3,
   followInfo:2,
   logoutInfo:1,
   loginInfo:0
  },[["LoginVerify",[["$0","loginInfo",Provider.Id(),0]]],["Logout",[["$0","logoutInfo",Provider.Id(),0]]],["FollowOperation",[["$0","followInfo",Provider.Id(),0]]],["TweetContent",[["$0","content",Provider.Id(),0]]],["QryFollowingName",[["$0","name",Provider.Id(),0]]],["QryHashtag",[["$0","hashtag",Provider.Id(),0]]],["QryMention",[["$0","mention",Provider.Id(),0]]]]))();
 };
 SimpleTwitter_Templates.main=function(h)
 {
  Templates.LoadLocalTemplates("main");
  return h?Templates.NamedTemplate("main",{
   $:1,
   $0:"main"
  },h):void 0;
 };
}(self));
