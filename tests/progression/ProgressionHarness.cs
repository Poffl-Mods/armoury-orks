// Test doubles model only the API paths verified in the installed assemblies.
// These tests exercise the real controller, not the game's event/serialization runtime.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ArmouryOrks.Runtime;
namespace UnityEngine {
 public class MonoBehaviour { protected static T FindFirstObjectByType<T>() where T:class {return null;} protected static void DontDestroyOnLoad(object o){} }
 public class GameObject {public GameObject(string s){} public void AddComponent<T>(){} }
 public enum RuntimeInitializeLoadType {AfterSceneLoad}
 public class RuntimeInitializeOnLoadMethodAttribute:Attribute {public RuntimeInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType t){} }
 public static class Time {public static float unscaledTime;}
 public static class Debug {public static void Log(object o){} public static void LogError(object o){} }
}
namespace Kingmaker.Blueprints.Items {public class BlueprintItem {public string AssetGuid;}}
namespace Kingmaker.Blueprints {public static class ResourcesLibrary {public static object TryGetBlueprint(string s){return new Items.BlueprintItem{AssetGuid=s};}}}
namespace Kingmaker.PubSubSystem {public interface ILevelUpCompleteUIHandler{} public interface IItemsCollectionHandler{} public interface IPartyCombatHandler{}}
namespace Kingmaker.PubSubSystem.Core {public static class EventBus{}}
namespace Kingmaker.EntitySystem.Entities {
 public class Inventory {public Kingmaker.Items.ItemsCollection Collection=new Kingmaker.Items.ItemsCollection();}
 public class BaseUnitEntity {public bool IsInCombat; public Inventory Inventory=new Inventory(); public Kingmaker.Items.PartUnitBody Body=new Kingmaker.Items.PartUnitBody();}
}
namespace Kingmaker {
 public class Game {public static Game Instance=new Game(); public Player Player=new Player();}
 public class Player {
  public Items.ItemsCollection Inventory=new Items.ItemsCollection(),SharedStash=new Items.ItemsCollection();
  public bool IsInCombat; public int PartyLevel=16;
  public List<EntitySystem.Entities.BaseUnitEntity> PartyAndPets=new List<EntitySystem.Entities.BaseUnitEntity>(),PartyAndPetsDetached=new List<EntitySystem.Entities.BaseUnitEntity>(),RemoteCompanions=new List<EntitySystem.Entities.BaseUnitEntity>();
 }
}
namespace Kingmaker.Items {
 public class PartUnitBody {public List<ItemEntity> Items=new List<ItemEntity>();}
 public class ItemEntity {
  public Kingmaker.Blueprints.Items.BlueprintItem Blueprint; public ItemsCollection Collection; public Slots.ItemSlot HoldingSlot;
  public int Count=1; public bool IsStackable,Disposed; public void Dispose(){Disposed=true;}
 }
 public static class ItemsEntityFactory {public static bool Fail; public static ItemEntity CreateEntity(Kingmaker.Blueprints.Items.BlueprintItem b){if(Fail)throw new Exception("factory");return new ItemEntity{Blueprint=b};}}
 public class ItemsCollection {
  public List<ItemEntity> Items=new List<ItemEntity>(); public Action<ItemEntity> AfterAdd; public bool IsVendorTable,FailAdd,FailAfterAdd,FailRemove,FailAfterRemove;
  public ItemEntity Add(ItemEntity i,bool noAutoMerge){if(FailAdd)throw new Exception("add");Items.Add(i);i.Collection=this;if(AfterAdd!=null)AfterAdd(i);if(FailAfterAdd)throw new Exception("post add");return i;}
  public ItemEntity Remove(ItemEntity i){if(FailRemove)throw new Exception("remove");Items.Remove(i);i.Collection=null;if(FailAfterRemove)throw new Exception("post remove");return i;}
 }
}
namespace Kingmaker.Items.Slots {
 public class ItemSlot {
  public Kingmaker.EntitySystem.Entities.BaseUnitEntity Owner;
  public Kingmaker.EntitySystem.Entities.Inventory MaybeOwnerInventory {get{return Owner.Inventory;}}
  public ItemEntity MaybeItem; public bool Refuse,FailInsert,Locked;
  public bool CanRemoveItem(){return !Locked;} public bool CanInsertItem(ItemEntity i){return !Locked;}
  public bool RemoveItem(bool a,bool b){if(Locked)return false;MaybeItem.HoldingSlot=null;MaybeItem=null;return true;}
  public void InsertItem(ItemEntity i,bool force){if(Refuse)return;if(FailInsert)throw new Exception("insert");MaybeItem=i;i.HoldingSlot=this;}
 }
}
class ProgressionHarness {
 const string Old="9f1a0cab53be493b96989c9a672d2ae5";
 static void Check(bool v,string message){if(!v)throw new Exception(message);}
 static void Run(string mode) {
  Kingmaker.Game.Instance.Player=new Kingmaker.Player();var p=Kingmaker.Game.Instance.Player;
  var c=new ChoppaProgressionController();var u=new Kingmaker.EntitySystem.Entities.BaseUnitEntity();p.PartyAndPets.Add(u);
  if(mode=="remote"){p.PartyAndPets.Clear();p.RemoteCompanions.Add(u);}
  if(mode=="detached"){p.PartyAndPets.Clear();p.PartyAndPetsDetached.Add(u);}
  if(mode=="combat")p.IsInCombat=true;
  var inv=mode=="inventory"?p.Inventory:mode=="stash"?p.SharedStash:u.Inventory.Collection;
  var old=new Kingmaker.Items.ItemEntity{Blueprint=new Kingmaker.Blueprints.Items.BlueprintItem{AssetGuid=Old}};inv.Add(old,true);
  var slot=new Kingmaker.Items.Slots.ItemSlot{Owner=u};
  if(mode!="inventory" && mode!="stash" && mode!="missing") {slot.InsertItem(old,false);u.Body.Items.Add(old);}
  if(mode=="missing"){inv.Remove(old);u.Body.Items.Add(old);}
  if(mode=="already")old.Blueprint.AssetGuid="78f71e9a52ab4f8e9a46bd2390860ec2";
  if(mode=="factory")Kingmaker.Items.ItemsEntityFactory.Fail=true;
  if(mode=="add")inv.FailAdd=true;
  if(mode=="postadd")inv.FailAfterAdd=true;
  if(mode=="refuse")slot.Refuse=true;
  if(mode=="insert")slot.FailInsert=true;
  if(mode=="locked")slot.Locked=true;
  if(mode=="remove")inv.FailRemove=true;
  if(mode=="postremove")inv.FailAfterRemove=true;
  if(mode=="stack")old.Count=2;
  var vendor=new Kingmaker.Items.ItemsCollection{IsVendorTable=mode!="ground"};var external=new Kingmaker.Items.ItemEntity{Blueprint=old.Blueprint};vendor.Add(external,true);var externalGuid=external.Blueprint.AssetGuid;
  c.HandleItemsAdded(vendor,external,1);
  Kingmaker.Items.ItemsEntityFactory.Fail=false;
  bool success=mode=="equipped"||mode=="inventory"||mode=="postremove"||mode=="remote"||mode=="detached"||mode=="stash"||mode=="ground";
  Check(vendor.Items.Count==1 && external.Blueprint.AssetGuid==externalGuid,"vendor changed");
  if(mode=="already")Check(inv.Items.Count==1 && inv.Items[0]==old,"already upgraded duplicated");
  else if(mode=="missing")Check(inv.Items.Count==0,"missing item recreated");
  else if(success){Check(inv.Items.Count==1 && inv.Items[0]!=old,"upgrade count");if(mode!="inventory" && mode!="stash")Check(slot.MaybeItem==inv.Items[0],"slot not preserved");c.HandleLevelUpComplete(false);Check(inv.Items.Count==1,"repeat duplicated");}
  else Check(inv.Items.Contains(old),"original lost: "+mode);
  if(mode!="remove" && !success && mode!="missing")Check(inv.Items.Count==1,"failed replacement leaked: "+mode);
  if(mode=="remove") {int count=inv.Items.Count;c.HandleLevelUpComplete(false);Check(inv.Items.Count==count,"recovery failure retried");}
  // No remembered vendor or prior-save collection may be touched by later passes.
  Kingmaker.Game.Instance.Player=new Kingmaker.Player();c.HandleLevelUpComplete(false);Check(vendor.Items.Count==1,"stale collection");
 }
 static void Movement() {
  Kingmaker.Game.Instance.Player=new Kingmaker.Player();var inv=Kingmaker.Game.Instance.Player.Inventory;
  var old=new Kingmaker.Items.ItemEntity{Blueprint=new Kingmaker.Blueprints.Items.BlueprintItem{AssetGuid=Old}};inv.Add(old,true);
  var outside=new Kingmaker.Items.ItemsCollection();
  inv.AfterAdd=i=>{inv.Remove(old);outside.Add(old,true);};
  new ChoppaProgressionController().HandleLevelUpComplete(false);
  Check(outside.Items.Count==1 && old.Collection==outside && inv.Items.Count==0,"callback moved original corrupted");
  Console.WriteLine("PASS callback movement");
 }
 static void Main(){Movement();foreach(var mode in new[]{"equipped","inventory","missing","already","factory","add","postadd","refuse","insert","locked","remove","postremove","stack","remote","detached","stash","ground","combat"}){Run(mode);Console.WriteLine("PASS "+mode);}}
}
