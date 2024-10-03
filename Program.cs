using System;
using System.Collections.Generic;
using System.IO;

public abstract class Entity
{
    public int Id { get; set; }

    public override string ToString()
    {
        return $"{Id}";
    }
}

public class Student : Entity
{
    public string Name { get; set; }
    public List<int> Courses { get; set; } = new List<int>();

    public Student(int studentId, string name)
    {
        Id = studentId;
        Name = name;
    }

    public void AddCourse(int courseId)
    {
        Courses.Add(courseId);
    }

    public override string ToString()
    {
        return $"Student Id = {Id}, Name = {Name}, Courses = {string.Join(" ", Courses)}";
    }
}

public class Teacher : Entity
{
    public string Name { get; set; }
    public int Experience { get; set; }
    public List<string> Courses { get; set; } = new List<string>();

    public Teacher(int teacherId, string name, int experience)
    {
        Id = teacherId;
        Name = name;
        Experience = experience;
    }

    public void AddCourse(string courseName)
    {
        Courses.Add(courseName);
    }

    public override string ToString()
    {
        return $"Teacher Id = {Id}, Exp = {Experience}, Name = {Name}, Lesson = {string.Join(" ", Courses)}";
    }
}

public class Course : Entity
{
    public string Name { get; set; }
    public List<int> Students { get; set; } = new List<int>();

    public Course(int courseId, string name)
    {
        Id = courseId;
        Name = name;
    }

    public void AddStudent(int studentId)
    {
        Students.Add(studentId);
    }

    public override string ToString()
    {
        return $"Course Id = {Id}, Name = {Name}, Course = {string.Join(" ", Students)}";
    }
}

public abstract class EntityFactory
{
    public abstract Entity Create(string[] attributes);
}

public class StudentFactory : EntityFactory
{
    public override Entity Create(string[] attributes)
    {
        int studentId = int.Parse(attributes[1]);
        string name = attributes[2];
        return new Student(studentId, name);
    }
}

public class TeacherFactory : EntityFactory
{
    public override Entity Create(string[] attributes)
    {
        int teacherId = int.Parse(attributes[1]);
        int experience = int.Parse(attributes[2]);
        string name = attributes[3];
        return new Teacher(teacherId, name, experience);
    }
}

public class CourseFactory : EntityFactory
{
    public override Entity Create(string[] attributes)
    {
        int courseId = int.Parse(attributes[1]);
        string name = attributes[2];
        return new Course(courseId, name);
    }
}

public class Database
{
    public Dictionary<int, Student> Students { get; set; } = new Dictionary<int, Student>();
    public Dictionary<int, Teacher> Teachers { get; set; } = new Dictionary<int, Teacher>();
    public Dictionary<int, Course> Courses { get; set; } = new Dictionary<int, Course>();

    private StudentFactory StudentFactory = new StudentFactory();
    private TeacherFactory TeacherFactory = new TeacherFactory();
    private CourseFactory CourseFactory = new CourseFactory();

    public void LoadFromFile(string filename)
    {
        if (!File.Exists(filename))
        {
            File.Create(filename);
            return;
        }

        foreach (var line in File.ReadLines(filename))
        {
            var attributes = line.Split(' ');
            var entityType = attributes[0];

            if (entityType == "student")
            {
                var student = (Student)StudentFactory.Create(attributes);
                Students[student.Id] = student;
            }
            else if (entityType == "teacher")
            {
                var teacher = (Teacher)TeacherFactory.Create(attributes);
                Teachers[teacher.Id] = teacher;
            }
            else if (entityType == "course")
            {
                var course = (Course)CourseFactory.Create(attributes);
                Courses[course.Id] = course;

                for (int i = 3; i < attributes.Length; i++) // Added handling for students
                {
                    if (int.TryParse(attributes[i], out var studentId))
                    {
                        course.AddStudent(studentId);
                        if (Students.ContainsKey(studentId))
                            Students[studentId].AddCourse(course.Id);
                    }
                }
            }
        }
    }

    public void SaveToFile(string filename)
    {
        using (var writer = new StreamWriter(filename))
        {
            foreach (var student in Students.Values)
            {
                writer.WriteLine(student.ToString());
            }

            foreach (var teacher in Teachers.Values)
            {
                writer.WriteLine(teacher.ToString());
            }

            foreach (var course in Courses.Values)
            {
                writer.WriteLine(course.ToString());
            }
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        Database db = new Database();

        // Загрузка из файла
        db.LoadFromFile("database.txt");

        // Пример добавления данных
        var teacher1 = new Teacher(1, "Jane Doe", 5);
        db.Teachers[teacher1.Id] = teacher1;

        var student1 = new Student(1, "Clara");
        var student2 = new Student(2, "Domi");
        db.Students[student1.Id] = student1;
        db.Students[student2.Id] = student2;

        var course1 = new Course(1, "Math");
        db.Courses[course1.Id] = course1;

        // Добавляем студентов к курсу
        course1.AddStudent(student1.Id);
        course1.AddStudent(student2.Id);
        student1.AddCourse(course1.Id);
        student2.AddCourse(course1.Id);

        // Добавляем курс для учителя по названию курса
        teacher1.AddCourse(course1.Name);

        // Сохранение в файл
        db.SaveToFile("database.txt");
    }
}