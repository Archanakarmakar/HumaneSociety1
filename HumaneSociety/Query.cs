using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumaneSociety
{
    public static class Query
    {
        static HumaneSocietyDataContext db;

        static Query()
        {
            db = new HumaneSocietyDataContext();
        }

        internal static List<USState> GetStates()
        {
            List<USState> allStates = db.USStates.ToList();
            return allStates;
        }

        internal static Client GetClient(string userName, string password)
        {
            Client client = db.Clients.Where(c => c.UserName == userName && c.Password == password).Single();

            return client;
        }

        internal static List<Client> GetClients()
        {
            List<Client> allClients = db.Clients.ToList();

            return allClients;
        }

        internal static void AddNewClient(string firstName, string lastName, string username, string password, string email, string streetAddress, int zipCode, int stateId)
        {
            Client newClient = new Client();

            newClient.FirstName = firstName;
            newClient.LastName = lastName;
            newClient.UserName = username;
            newClient.Password = password;
            newClient.Email = email;

            Address addressFromDb = db.Addresses.Where(a => a.AddressLine1 == streetAddress && a.Zipcode == zipCode && a.USStateId == stateId).FirstOrDefault();

            // if the address isn't found in the Db, create and insert it
            if (addressFromDb == null)
            {
                Address newAddress = new Address();
                newAddress.AddressLine1 = streetAddress;
                newAddress.City = null;
                newAddress.USStateId = stateId;
                newAddress.Zipcode = zipCode;

                db.Addresses.InsertOnSubmit(newAddress);
                db.SubmitChanges();

                addressFromDb = newAddress;
            }

            // attach AddressId to clientFromDb.AddressId
            newClient.AddressId = addressFromDb.AddressId;

            db.Clients.InsertOnSubmit(newClient);

            db.SubmitChanges();
        }

        internal static void UpdateClient(Client clientWithUpdates)
        {
            // find corresponding Client from Db
            Client clientFromDb = null;

            try
            {
                clientFromDb = db.Clients.Where(c => c.ClientId == clientWithUpdates.ClientId).Single();
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("No clients have a ClientId that matches the Client passed in.");
                Console.WriteLine("No update have been made.");
                return;
            }

            // update clientFromDb information with the values on clientWithUpdates (aside from address)
            clientFromDb.FirstName = clientWithUpdates.FirstName;
            clientFromDb.LastName = clientWithUpdates.LastName;
            clientFromDb.UserName = clientWithUpdates.UserName;
            clientFromDb.Password = clientWithUpdates.Password;
            clientFromDb.Email = clientWithUpdates.Email;

            // get address object from clientWithUpdates
            Address clientAddress = clientWithUpdates.Address;

            // look for existing Address in Db (null will be returned if the address isn't already in the Db
            Address updatedAddress = db.Addresses.Where(a => a.AddressLine1 == clientAddress.AddressLine1 && a.USStateId == clientAddress.USStateId && a.Zipcode == clientAddress.Zipcode).FirstOrDefault();

            // if the address isn't found in the Db, create and insert it
            if (updatedAddress == null)
            {
                Address newAddress = new Address();
                newAddress.AddressLine1 = clientAddress.AddressLine1;
                newAddress.City = null;
                newAddress.USStateId = clientAddress.USStateId;
                newAddress.Zipcode = clientAddress.Zipcode;

                db.Addresses.InsertOnSubmit(newAddress);
                db.SubmitChanges();

                updatedAddress = newAddress;
            }

            // attach AddressId to clientFromDb.AddressId
            clientFromDb.AddressId = updatedAddress.AddressId;

            // submit changes
            db.SubmitChanges();
        }

        internal static void AddUsernameAndPassword(Employee employee)
        {
            Employee employeeFromDb = db.Employees.Where(e => e.EmployeeId == employee.EmployeeId).FirstOrDefault();

            employeeFromDb.UserName = employee.UserName;
            employeeFromDb.Password = employee.Password;

            db.SubmitChanges();
        }

        internal static Employee RetrieveEmployeeUser(string email, int employeeNumber)
        {
            Employee employeeFromDb = db.Employees.Where(e => e.Email == email && e.EmployeeNumber == employeeNumber).FirstOrDefault();

            if (employeeFromDb == null)
            {
                throw new NullReferenceException();
            }
            else
            {
                return employeeFromDb;
            }
        }

        internal static Employee EmployeeLogin(string userName, string password)
        {
            Employee employeeFromDb = db.Employees.Where(e => e.UserName == userName && e.Password == password).FirstOrDefault();

            return employeeFromDb;
        }

        internal static bool CheckEmployeeUserNameExist(string userName)
        {
            Employee employeeWithUserName = db.Employees.Where(e => e.UserName == userName).FirstOrDefault();
            return employeeWithUserName == null;
        }


        //// TODO Items: ////

        // TODO: Allow any of the CRUD operations to occur here
        internal static void RunEmployeeQueries(Employee employee, string crudOperation)
        {
            switch (crudOperation)
            {
                case "create":
                    var employeeNumberCheck = db.Employees.Where(e => e.EmployeeNumber == employee.EmployeeNumber).FirstOrDefault();
                    if (employeeNumberCheck == null)
                    {
                        var employeeEmailCheck = db.Employees.Where(e => e.Email == employee.Email).FirstOrDefault();
                        if (employeeEmailCheck == null)
                        {
                            db.Employees.InsertOnSubmit(employee);
                            db.SubmitChanges();
                        }
                        else
                        {
                            UserInterface.DisplayUserOptions("There is already an employee in the database with that employee email.");

                        }
                    }
                    else
                    {
                        UserInterface.DisplayUserOptions("There is already an employee in the database with that employee number.");

                    }
                    break;
                case "delete":
                    employee = db.Employees.Where(e => e.LastName == employee.LastName && e.EmployeeNumber == employee.EmployeeNumber).SingleOrDefault();
                    db.Employees.DeleteOnSubmit(employee);
                    db.SubmitChanges();
                    break;
                case "read":
                    employee = db.Employees.Where(e => e.EmployeeNumber == employee.EmployeeNumber).FirstOrDefault();
                    List<string> readOptions = new List<string>() {
                        "----------------------------------------------",
                        "  Current Employee Details:",
                        "----------------------------------------------",
                        "     Employee Name: " + employee.FirstName + " " + employee.LastName,
                        "     Employee Username: " + employee.UserName,
                        "     Employee Password: " + employee.Password,
                        "     Employee Number: " + employee.EmployeeNumber,
                        "     Employee Email: " + employee.Email,
                        "------------------------------------------------"
                        };
                    UserInterface.DisplayUserOptions(readOptions);
                    Console.ReadKey();
                    break;

                case "update":

                    employee = db.Employees.Where(e => e.EmployeeNumber == employee.EmployeeNumber).SingleOrDefault();
                    List<string> currentUpdateOptions = new List<string>() {
                        "----------------------------------------------",
                        "  Current Employee Details:",
                        "----------------------------------------------",
                        "     Employee Name: " + employee.FirstName + " " + employee.LastName,
                        "     Employee Username: " + employee.UserName,
                        "     Employee Password: " + employee.Password,
                        "     Employee Number: " + employee.EmployeeNumber,
                        "     Employee Email: " + employee.Email,
                        "------------------------------------------------"
                    };
                    UserInterface.DisplayUserOptions(currentUpdateOptions);
                    employee.FirstName = UserInterface.GetStringData("first name", "the employee's new");
                    employee.LastName = UserInterface.GetStringData("last name", "the employee's new");
                    employee.EmployeeNumber = int.Parse(UserInterface.GetStringData("employee number", "the employee's new"));
                    employee.Email = UserInterface.GetStringData("email", "the employee's new");

                    List<string> newUpdateOptions = new List<string>() {
                        "----------------------------------------------",
                        "  New Employee Details:",
                        "----------------------------------------------",
                        "     Employee Name: " + employee.FirstName + " " + employee.LastName,
                        "     Employee Username: " + employee.UserName,
                        "     Employee Password: " + employee.Password,
                        "     Employee Number: " + employee.EmployeeNumber,
                        "     Employee Email: " + employee.Email,
                        "----------------------------------------------"
                    };
                    break;
            }
            UserInterface.DisplayUserOptions("Would you like to save these changes?");
            var input = UserInterface.GetBitData();
            if (input == true)
            {
                db.SubmitChanges();
            }

        }
        // TODO: Animal CRUD Operations
        internal static void AddAnimal(Animal animal)
        {
            db.Animals.InsertOnSubmit(animal);
            db.SubmitChanges();
        }



        internal static Animal GetAnimalByID(int id)
        {

            Animal result = new Animal();
            result = db.Animals.Where(a => a.AnimalId == id).FirstOrDefault();
            return result;
        }


        internal static void UpdateAnimal(int animalId, Dictionary<int, string> updates)
        {

            new NotImplementedException();

            
            var animal = db.Animals.Where(a => a.AnimalId == animalId).FirstOrDefault();
            foreach (KeyValuePair<int, string> info in updates)
            {
                switch (info.Key)
                {
                    //"1. Category", "2. Name", "3. Age", "4. Demeanor", "5. Kid friendly", "6. Pet friendly", "7. Weight",
                    case 1:
                        animal.CategoryId = Convert.ToInt32(info.Value);
                        break;
                    case 2:
                        animal.Name = info.Value;
                        break;
                    case 3:
                        animal.Age = Convert.ToInt32(info.Value);
                        break;
                    case 4:
                        animal.Demeanor = info.Value;
                        break;
                    case 5:
                        animal.KidFriendly = Convert.ToBoolean(info.Value);
                        break;
                    case 6:
                        animal.PetFriendly = Convert.ToBoolean(info.Value);
                        break;
                    case 7:
                        animal.Weight = Convert.ToInt32(info.Value);
                        break;
                }

            }
            db.SubmitChanges();

        }

        internal static void RemoveAnimal(Animal animal)
        {
            Animal animalDelete = db.Animals.Where(a => a == animal).FirstOrDefault();
            db.Animals.DeleteOnSubmit(animalDelete);

        }

        // TODO: Animal Multi-Trait Search
        internal static IQueryable<Animal> SearchForAnimalsByMultipleTraits(Dictionary<int, string> updates) // parameter(s)?
        {
            var animal = db.Animals.AsQueryable();
            foreach (KeyValuePair<int,string> info in updates)
            {
                switch (info.Key)
                {
                    case 1:
                        animal = db.Animals.Where(a => a.CategoryId == Convert.ToInt32(info.Value));
                        break;
                    case 2:
                        animal = db.Animals.Where(a => a.Name == info.Value);
                        break;
                    case 3:
                        animal = db.Animals.Where(a => a.Age == Convert.ToInt32(info.Value));
                        break;
                    case 4:
                        animal = db.Animals.Where(a => a.Demeanor == info.Value);
                        break;
                    case 5:
                        animal = db.Animals.Where(a => a.KidFriendly == Convert.ToBoolean(info.Value));
                        break;
                    case 6:
                        animal = db.Animals.Where(a => a.PetFriendly == Convert.ToBoolean(info.Value));
                        break;
                    case 7:
                        animal = db.Animals.Where(a => a.Weight == Convert.ToInt32(info.Value));
                        break;
                    case 8:
                        animal = db.Animals.Where(a => a.AnimalId == Convert.ToInt32(info.Value));
                        break;
                }
                                                  
            }
            return animal;

        }

        // TODO: Misc Animal Things
        internal static int GetCategoryId(string categoryName)
        {
            int categoryID;
            categoryID = db.Categories.Where(n => n.Name == categoryName).Select(n => n.CategoryId).FirstOrDefault();
            return categoryID;
        }

        internal static Room GetRoom(int animalId)
        {
            Room room = db.Rooms.Where(r => r.AnimalId == animalId).FirstOrDefault();
            return room;
        }

        internal static int GetDietPlanId(string dietPlanName)
        {
            int dietname;
            dietname = db.DietPlans.Where(f => f.FoodType == dietPlanName).Select(f => f.DietPlanId).FirstOrDefault();
            return dietname;

        }

        // TODO: Adoption CRUD Operations
        internal static void Adopt(Animal animal, Client client)
        {

            Adoption newAdoption = new Adoption();
            newAdoption.ClientId = client.ClientId;
            newAdoption.AnimalId = animal.AnimalId;
            newAdoption.ApprovalStatus = "Pending";
            newAdoption.AdoptionFee = 75;
            newAdoption.PaymentCollected = false;

            db.Adoptions.InsertOnSubmit(newAdoption);
            db.SubmitChanges();

            

        }


        internal static IQueryable<Adoption> GetPendingAdoptions()
        {
            return db.Adoptions.Where(x => x.ApprovalStatus == "pending");

        }

        internal static void UpdateAdoption(bool isAdopted, Adoption adoption)
        {
            Adoption DbAdoption = db.Adoptions.Where(s => s.AnimalId == adoption.AnimalId).Single();

            if (isAdopted)
            {
                DbAdoption.ApprovalStatus = "Approved";
            }
            else
            {
                DbAdoption.ApprovalStatus = "Denied";
            }

            db.Adoptions.InsertOnSubmit(DbAdoption);
            db.SubmitChanges();
        }



        internal static void RemoveAdoption(int animalId, int clientId)
        {
            Adoption adoptedanimalDelete = db.Adoptions.Where(ad => ad.AnimalId == animalId && ad.ClientId == clientId).Select(ad => ad).FirstOrDefault();
            db.Adoptions.DeleteOnSubmit(adoptedanimalDelete);

        }
        // TODO: Shot
        internal static IQueryable<AnimalShot> GetShots(Animal animal)
        {
            var result = db.AnimalShots.AsQueryable();
            result = db.AnimalShots.Where(a => a.AnimalId == animal.AnimalId).Select(a => a);
            return result;
        }

        internal static void UpdateShot(string shotName, Animal animal)
        {
            DateTime now = DateTime.Now;
            AnimalShot animalShot = new AnimalShot();
            animalShot.AnimalId = animal.AnimalId;
            animalShot.ShotId = db.Shots.Where( s => s.Name == shotName).Select(s=>s.ShotId).FirstOrDefault();
            animalShot.DateReceived = now;
            db.AnimalShots.InsertOnSubmit(animalShot);
            db.SubmitChanges();

        }
    }
}

