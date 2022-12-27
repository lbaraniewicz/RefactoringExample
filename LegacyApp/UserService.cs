using System;
using System.Reflection.Metadata;

namespace LegacyApp
{
    public class UserService
    {
        //AddUser_ShouldAddUserCorrectly
        //AddUser_ShouldFail_IncorrectEmail

        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            if (!ValidateUserData(firstName, lastName, email))
            {
                return false;
            }

            if (!CheckUserAge(dateOfBirth))
            {
                return false;
            }

            var clientRepository = new ClientRepository();
            var client = clientRepository.GetById(clientId);

            var user = new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName
            };

            SetUserCreditLimit(user, client);

            if (!CheckUserLimit(user))
            {
                return false;
            }

            UserDataAccess.AddUser(user);
            return true;
        }

        private bool CheckUserLimit(User user)
        {
            if (user.HasCreditLimit && user.CreditLimit < 500)
            {
                return false;
            }
            return true;
        }

        private void SetUserCreditLimit(User user, Client client)
        {
            if (client.Name == "VeryImportantClient")
            {
                //Skip credit limit
                user.HasCreditLimit = false;
            }
            else if (client.Name == "ImportantClient")
            {
                using (var userCreditService = new UserCreditService())
                {
                    int creditLimit = userCreditService.GetCreditLimit(user.FirstName, user.LastName, user.DateOfBirth);
                    creditLimit = creditLimit * 2;
                    user.CreditLimit = creditLimit;
                }
            }
            else
            {
                //Do credit check
                user.HasCreditLimit = true;
                using (var userCreditService = new UserCreditService())
                {
                    int creditLimit = userCreditService.GetCreditLimit(user.FirstName, user.LastName, user.DateOfBirth);
                    user.CreditLimit = creditLimit;
                }
            }
        }

        private bool CheckUserAge(DateTime dateOfBirth)
        {
            var now = DateTime.Now;
            int age = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day)) age--;

            if (age < 21)
            {
                return false;
            }
            return true;
        }

        private bool ValidateUserData(string firstName, string lastName, string email)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                return false;
            }

            //Walidacja email'a
            if (!email.Contains("@") && !email.Contains("."))
            {
                return false;
            }
            return true;
        }
    }
}
