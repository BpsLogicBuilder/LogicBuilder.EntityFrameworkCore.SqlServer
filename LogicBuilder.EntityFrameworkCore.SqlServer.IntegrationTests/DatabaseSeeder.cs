using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Data;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Models;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Models.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests
{
    internal static class DatabaseSeeder
    {
        internal static async Task Seed_Database(IDataClassesRepository repository)
        {
            if ((await repository.CountAsync<AddressModel, Address>()) > 0)
                return;//database has been seeded

            AddressModel[] addresses =
            [
                new AddressModel { City = "CityOne", EntityState = Domain.EntityStateType.Added },
                new AddressModel { City = "CityTwo", EntityState = Domain.EntityStateType.Added },
                new AddressModel { City = "CityThree", EntityState = Domain.EntityStateType.Added },
                new AddressModel { City = "CityFour", EntityState = Domain.EntityStateType.Added },
                new AddressModel { City = "CityFive", EntityState = Domain.EntityStateType.Added },
                new AddressModel { City = "A", EntityState = Domain.EntityStateType.Added },
                new AddressModel { City = "B", EntityState = Domain.EntityStateType.Added },
                new AddressModel { City = "C", EntityState = Domain.EntityStateType.Added },
                new AddressModel { City = "D", EntityState = Domain.EntityStateType.Added },
                new AddressModel { City = "E", EntityState = Domain.EntityStateType.Added }
            ];

            await repository.SaveGraphsAsync<AddressModel, Address>(addresses);



            CategoryModel[] categories =
            [
                new CategoryModel 
                { 
                    CategoryName = "CategoryOne",
                    EntityState = Domain.EntityStateType.Added
                },
                new CategoryModel
                { 
                    CategoryName = "CategoryTwo",
                    EntityState = Domain.EntityStateType.Added
                }
            ];

            await repository.SaveGraphsAsync<CategoryModel, Category>(categories);

            ProductModel[] products =
            [
                new ProductModel
                {
                    ProductName = "ProductOne",
                    CategoryID = categories.Single(c => c.CategoryName == "CategoryOne").CategoryID,
                    SupplierID = addresses.Single(a => a.City == "A").AddressID,
                    EntityState = Domain.EntityStateType.Added
                },
                new ProductModel
                {
                    ProductName = "ProductTwo",
                    CategoryID = categories.Single(c => c.CategoryName == "CategoryOne").CategoryID,
                    SupplierID  = addresses.Single(a => a.City == "B").AddressID,
                    EntityState = Domain.EntityStateType.Added
                },
                new ProductModel
                {
                    ProductName = "ProductThree",
                    CategoryID = categories.Single(c => c.CategoryName == "CategoryOne").CategoryID,
                    SupplierID = addresses.Single(a => a.City == "B").AddressID,
                    EntityState = Domain.EntityStateType.Added
                },
                new ProductModel
                {
                    ProductName = "ProductFour",
                    CategoryID = categories.Single(c => c.CategoryName == "CategoryTwo").CategoryID,
                    SupplierID  = addresses.Single(a => a.City == "D").AddressID,
                    EntityState = Domain.EntityStateType.Added
                },
                new ProductModel
                {
                    ProductName = "ProductFive",
                    CategoryID = categories.Single(c => c.CategoryName == "CategoryTwo").CategoryID,
                    SupplierID  = addresses.Single(a => a.City == "E").AddressID,
                    EntityState = Domain.EntityStateType.Added
                }
            ];

            await repository.SaveGraphsAsync<ProductModel, Product>(products);

            int productOneID = products.Single(p => p.ProductName == "ProductOne").ProductID;
            int productFourID = products.Single(p => p.ProductName == "ProductFour").ProductID;
            AlternateAddressModel[] alternateAddresses =
            [
                new AlternateAddressModel { City = "CityOne", ProductID = productOneID, EntityState = Domain.EntityStateType.Added },
                new AlternateAddressModel { City = "CityTwo", ProductID = productOneID, EntityState = Domain.EntityStateType.Added },
                new AlternateAddressModel { City = "CityThree", ProductID = productFourID, EntityState = Domain.EntityStateType.Added },
                new AlternateAddressModel { City = "CityFour", ProductID = productFourID, EntityState = Domain.EntityStateType.Added },
                new AlternateAddressModel { City = "CityFive", ProductID = productFourID, EntityState = Domain.EntityStateType.Added },
            ];

            await repository.SaveGraphsAsync<AlternateAddressModel, AlternateAddress>(alternateAddresses);


        }

        internal static async Task Seed_Database(ISchoolRepository repository)
        {
            if ((await repository.CountAsync<StudentModel, Student>()) > 0)
                return;//database has been seeded

            InstructorModel[] instructors =
            [
                new InstructorModel { FirstName = "Roger",   LastName = "Zheng", HireDate = DateTime.Parse("2004-02-12", CultureInfo.CurrentCulture), EntityState = LogicBuilder.Domain.EntityStateType.Added },
                new InstructorModel { FirstName = "Kim", LastName = "Abercrombie", HireDate = DateTime.Parse("1995-03-11", CultureInfo.CurrentCulture), EntityState = LogicBuilder.Domain.EntityStateType.Added},
                new InstructorModel { FirstName = "Fadi", LastName = "Fakhouri", HireDate = DateTime.Parse("2002-07-06", CultureInfo.CurrentCulture), OfficeAssignment = new OfficeAssignmentModel { Location = "Smith 17" }, EntityState = LogicBuilder.Domain.EntityStateType.Added},
                new InstructorModel { FirstName = "Roger", LastName = "Harui", HireDate = DateTime.Parse("1998-07-01", CultureInfo.CurrentCulture), OfficeAssignment = new OfficeAssignmentModel { Location = "Gowan 27" }, EntityState = LogicBuilder.Domain.EntityStateType.Added },
                new InstructorModel { FirstName = "Candace", LastName = "Kapoor", HireDate = DateTime.Parse("2001-01-15", CultureInfo.CurrentCulture), OfficeAssignment = new OfficeAssignmentModel { Location = "Thompson 304" }, EntityState = LogicBuilder.Domain.EntityStateType.Added }
            ];
            await repository.SaveGraphsAsync<InstructorModel, Instructor>(instructors);

            DepartmentModel[] departments =
            [
                new DepartmentModel
                {
                    EntityState = Domain.EntityStateType.Added,
                    Name = "English",     Budget = 350000,
                    StartDate = DateTime.Parse("2007-09-01", CultureInfo.CurrentCulture),
                    InstructorID = instructors.Single(i => i.FirstName == "Kim" && i.LastName == "Abercrombie").ID,
                    Courses =  new HashSet<CourseModel>
                    {
                        new() {CourseID = 2021, Title = "Composition",    Credits = 3},
                        new() {CourseID = 2042, Title = "Literature",     Credits = 4}
                    }
                },
                new DepartmentModel
                {
                    EntityState = LogicBuilder.Domain.EntityStateType.Added,
                    Name = "Mathematics",
                    Budget = 100000,
                    StartDate = DateTime.Parse("2007-09-01", CultureInfo.CurrentCulture),
                    InstructorID = instructors.Single(i => i.FirstName == "Fadi" && i.LastName == "Fakhouri").ID,
                    Courses =  new HashSet<CourseModel>
                    {
                        new() {CourseID = 1045, Title = "Calculus",       Credits = 4},
                        new() {CourseID = 3141, Title = "Trigonometry",   Credits = 4}
                    }
                },
                new DepartmentModel
                {
                    EntityState = LogicBuilder.Domain.EntityStateType.Added,
                    Name = "Engineering", Budget = 350000,
                    StartDate = DateTime.Parse("2007-09-01", CultureInfo.CurrentCulture),
                    InstructorID = instructors.Single(i => i.FirstName == "Roger" && i.LastName == "Harui").ID,
                    Courses =  new HashSet<CourseModel>
                    {
                        new() {CourseID = 1050, Title = "Chemistry",      Credits = 3}
                    }
                },
                new DepartmentModel
                {
                    EntityState = LogicBuilder.Domain.EntityStateType.Added,
                    Name = "Economics",
                    Budget = 100000,
                    StartDate = DateTime.Parse("2007-09-01", CultureInfo.CurrentCulture),
                    InstructorID = instructors.Single(i => i.FirstName == "Candace" && i.LastName == "Kapoor").ID,
                    Courses =  new HashSet<CourseModel>
                    {
                        new() {CourseID = 4022, Title = "Microeconomics", Credits = 3},
                        new() {CourseID = 4041, Title = "Macroeconomics", Credits = 3 }
                    }
                }
            ];
            await repository.SaveGraphsAsync<DepartmentModel, Department>(departments);

            IEnumerable<CourseModel> courses = departments.SelectMany(d => d.Courses);
            CourseAssignmentModel[] courseInstructors =
            [
                new CourseAssignmentModel {
                    EntityState = LogicBuilder.Domain.EntityStateType.Added,
                    CourseID = courses.Single(c => c.Title == "Chemistry" ).CourseID,
                    InstructorID = instructors.Single(i => i.LastName == "Kapoor").ID
                    },
                new CourseAssignmentModel {
                    EntityState = LogicBuilder.Domain.EntityStateType.Added,
                    CourseID = courses.Single(c => c.Title == "Chemistry" ).CourseID,
                    InstructorID = instructors.Single(i => i.LastName == "Harui").ID
                    },
                new CourseAssignmentModel {
                    EntityState = LogicBuilder.Domain.EntityStateType.Added,
                    CourseID = courses.Single(c => c.Title == "Microeconomics" ).CourseID,
                    InstructorID = instructors.Single(i => i.LastName == "Zheng").ID
                    },
                new CourseAssignmentModel {
                    EntityState = LogicBuilder.Domain.EntityStateType.Added,
                    CourseID = courses.Single(c => c.Title == "Macroeconomics" ).CourseID,
                    InstructorID = instructors.Single(i => i.LastName == "Zheng").ID
                    },
                new CourseAssignmentModel {
                    EntityState = LogicBuilder.Domain.EntityStateType.Added,
                    CourseID = courses.Single(c => c.Title == "Calculus" ).CourseID,
                    InstructorID = instructors.Single(i => i.LastName == "Fakhouri").ID
                    },
                new CourseAssignmentModel {
                    EntityState = LogicBuilder.Domain.EntityStateType.Added,
                    CourseID = courses.Single(c => c.Title == "Trigonometry" ).CourseID,
                    InstructorID = instructors.Single(i => i.LastName == "Harui").ID
                    },
                new CourseAssignmentModel {
                    EntityState = LogicBuilder.Domain.EntityStateType.Added,
                    CourseID = courses.Single(c => c.Title == "Composition" ).CourseID,
                    InstructorID = instructors.Single(i => i.LastName == "Abercrombie").ID
                    },
                new CourseAssignmentModel {
                    EntityState = LogicBuilder.Domain.EntityStateType.Added,
                    CourseID = courses.Single(c => c.Title == "Literature" ).CourseID,
                    InstructorID = instructors.Single(i => i.LastName == "Abercrombie").ID
                    },
            ];
            await repository.SaveGraphsAsync<CourseAssignmentModel, CourseAssignment>(courseInstructors);

            StudentModel[] students =
            [
                new StudentModel
                {
                    EntityState =  LogicBuilder.Domain.EntityStateType.Added,
                    FirstName = "Carson",   LastName = "Alexander",
                    EnrollmentDate = DateTime.Parse("2010-09-01", CultureInfo.CurrentCulture),
                    Enrollments = new HashSet<EnrollmentModel>
                    {
                        new() {
                            CourseID = courses.Single(c => c.Title == "Chemistry" ).CourseID,
                            Grade = Models.Grade.A
                        },
                        new() {
                            CourseID = courses.Single(c => c.Title == "Microeconomics" ).CourseID,
                            Grade = Models.Grade.C
                        },
                        new() {
                            CourseID = courses.Single(c => c.Title == "Macroeconomics" ).CourseID,
                            Grade = Models.Grade.B
                        }
                    }
                },
                new StudentModel
                {
                    EntityState =  LogicBuilder.Domain.EntityStateType.Added,
                    FirstName = "Meredith", LastName = "Alonso",
                    EnrollmentDate = DateTime.Parse("2012-09-01", CultureInfo.CurrentCulture),
                    Enrollments = new HashSet<EnrollmentModel>
                    {
                        new() {
                            CourseID = courses.Single(c => c.Title == "Calculus" ).CourseID,
                            Grade = Models.Grade.B
                        },
                        new() {
                            CourseID = courses.Single(c => c.Title == "Trigonometry" ).CourseID,
                            Grade = Models.Grade.B
                        },
                        new() {
                            CourseID = courses.Single(c => c.Title == "Composition" ).CourseID,
                            Grade = Models.Grade.B
                        }
                    }
                },
                new StudentModel
                {
                    EntityState =  LogicBuilder.Domain.EntityStateType.Added,
                    FirstName = "Arturo",   LastName = "Anand",
                    EnrollmentDate = DateTime.Parse("2013-09-01", CultureInfo.CurrentCulture),
                    Enrollments = new HashSet<EnrollmentModel>
                    {
                        new() {
                            CourseID = courses.Single(c => c.Title == "Chemistry" ).CourseID
                        },
                        new() {
                            CourseID = courses.Single(c => c.Title == "Microeconomics").CourseID,
                            Grade = Models.Grade.B
                        },
                    }
                },
                new StudentModel
                {
                    EntityState =  LogicBuilder.Domain.EntityStateType.Added,
                    FirstName = "Gytis",    LastName = "Barzdukas",
                    EnrollmentDate = DateTime.Parse("2012-09-01", CultureInfo.CurrentCulture),
                    Enrollments = new HashSet<EnrollmentModel>
                    {
                        new() {
                            CourseID = courses.Single(c => c.Title == "Chemistry").CourseID,
                            Grade = Models.Grade.B
                        }
                    }
                },
                new StudentModel
                {
                    EntityState =  LogicBuilder.Domain.EntityStateType.Added,
                    FirstName = "Yan",      LastName = "Li",
                    EnrollmentDate = DateTime.Parse("2012-09-01", CultureInfo.CurrentCulture),
                    Enrollments = new HashSet<EnrollmentModel>
                    {
                        new() {
                            CourseID = courses.Single(c => c.Title == "Composition").CourseID,
                            Grade = Models.Grade.B
                        }
                    }
                },
                new StudentModel
                {
                    EntityState =  LogicBuilder.Domain.EntityStateType.Added,
                    FirstName = "Peggy",    LastName = "Justice",
                    EnrollmentDate = DateTime.Parse("2011-09-01", CultureInfo.CurrentCulture),
                    Enrollments = new HashSet<EnrollmentModel>
                    {
                        new() {
                            CourseID = courses.Single(c => c.Title == "Literature").CourseID,
                            Grade = Models.Grade.B
                        }
                    }
                },
                new StudentModel
                {
                    EntityState =  LogicBuilder.Domain.EntityStateType.Added,
                    FirstName = "Laura",    LastName = "Norman",
                    EnrollmentDate = DateTime.Parse("2013-09-01", CultureInfo.CurrentCulture)
                },
                new StudentModel
                {
                    EntityState = LogicBuilder.Domain.EntityStateType.Added,
                    FirstName = "Nino",     LastName = "Olivetto",
                    EnrollmentDate = DateTime.Parse("2005-09-01", CultureInfo.CurrentCulture)
                },
                new StudentModel
                {
                    EntityState = LogicBuilder.Domain.EntityStateType.Added,
                    FirstName = "Tom",
                    LastName = "Spratt",
                    EnrollmentDate = DateTime.Parse("2010-09-01", CultureInfo.CurrentCulture),
                    Enrollments = new HashSet<EnrollmentModel>
                    {
                        new() {
                            CourseID = 1045,
                            Grade = Models.Grade.B
                        }
                    }
                },
                new StudentModel
                {
                    EntityState = LogicBuilder.Domain.EntityStateType.Added,
                    FirstName = "Billie",
                    LastName = "Spratt",
                    EnrollmentDate = DateTime.Parse("2010-09-01", CultureInfo.CurrentCulture),
                    Enrollments = new HashSet<EnrollmentModel>
                    {
                        new() {
                            CourseID = 1050,
                            Grade = Models.Grade.B
                        }
                    }
                },
                new StudentModel
                {
                    EntityState = LogicBuilder.Domain.EntityStateType.Added,
                    FirstName = "Jackson",
                    LastName = "Spratt",
                    EnrollmentDate = DateTime.Parse("2017-09-01", CultureInfo.CurrentCulture),
                    Enrollments = new HashSet<EnrollmentModel>
                    {
                        new() {
                            CourseID = 2021,
                            Grade = Models.Grade.B
                        }
                    }
                }
            ];

            await repository.SaveGraphsAsync<StudentModel, Student>(students);
        }
    }
}