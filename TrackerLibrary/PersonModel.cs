﻿using System;
using System.Collections.Generic;
using System.Linq;
namespace TrackerLibrary
{
    public class PersonModel
    {
        /// <summary>
        /// First name of person
        /// </summary>
        public string FirstName { get; set; }
        
        /// <summary>
        /// Last name of person
        /// </summary>
        public string LastName { get; set; }
        
        /// <summary>
        /// Person's email address
        /// </summary>
        public string EmailAddress { get; set; }
        
        /// <summary>
        /// Person's phone number
        /// </summary>
        public string PhoneNumber { get; set; }
    }
}
