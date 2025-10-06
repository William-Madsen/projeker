using System.Globalization;
using Dapper;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseCors();

var connStr = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(connStr))
    throw new Exception("Manglende ConnectionStrings:Default i appsettings.json");

var smtp = builder.Configuration.GetSection("Smtp");
string smtpHost  = smtp["Host"] ?? throw new Exception("Smtp:Host mangler");
int smtpPort     = int.TryParse(smtp["Port"], out var p) ? p : 587;
string smtpUser  = smtp["User"] ?? throw new Exception("Smtp:User mangler");
string smtpPass  = smtp["Pass"] ?? throw new Exception("Smtp:Pass mangler");
string fromEmail = smtp["FromEmail"] ?? throw new Exception("Smtp:FromEmail mangler");
string fromName  = smtp["FromName"] ?? "Booking";

app.MapGet("/", () => Results.Text("Booking API kører "));

app.MapPost("/book", async (HttpRequest req) =>
{
    if (!req.HasFormContentType) return Results.BadRequest(new { error = "Form data mangler" });

    var form = await req.ReadFormAsync();
    string name = form["name"].ToString().Trim();
    string email = form["email"].ToString().Trim();
    string checkin = form["checkin"].ToString().Trim();
    string checkout = form["checkout"].ToString().Trim();
    string room = form["room"].ToString().Trim();

    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email)
        || string.IsNullOrWhiteSpace(checkin) || string.IsNullOrWhiteSpace(checkout)
        || string.IsNullOrWhiteSpace(room))
        return Results.BadRequest(new { error = "Alle felter er påkrævet" });

    if (!DateOnly.TryParseExact(checkin, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var ci)
        || !DateOnly.TryParseExact(checkout, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var co)
        || ci >= co)
        return Results.BadRequest(new { error = "Ugyldige datoer (checkout skal være efter checkin)" });

    var checkinDate = ci.ToDateTime(TimeOnly.MinValue);
    var checkoutDate = co.ToDateTime(TimeOnly.MinValue);

    try
    {
        await using var conn = new MySqlConnection(connStr);
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();

        await conn.ExecuteAsync(
            @"INSERT INTO users (email, name) VALUES (@email, @name)
              ON DUPLICATE KEY UPDATE name = VALUES(name);",
            new { email, name }, tx);

        var userId = await conn.ExecuteScalarAsync<int>(
            "SELECT id FROM users WHERE email=@email LIMIT 1;",
            new { email }, tx);

        await conn.ExecuteAsync(
            @"INSERT INTO bookings (user_id, checkin, checkout, room)
              VALUES (@userId, @checkin, @checkout, @room);",
            new { userId, checkin = checkinDate, checkout = checkoutDate, room }, tx);

        await tx.CommitAsync();
    }
    catch (MySqlException ex)
    {
        Console.WriteLine("DB fejl: " + ex.Message);
        return Results.Problem("Kunne ikke gemme booking", statusCode: 500);
    }

    var msg = new MimeMessage();
    msg.From.Add(new MailboxAddress(fromName, fromEmail));
    msg.To.Add(new MailboxAddress(name, email));
    msg.Subject = "Booking bekræftet";

    var bodyText = $@"Hej {name},

Tak for din booking!

Detaljer:
- Check-in:  {ci:yyyy-MM-dd}
- Check-out: {co:yyyy-MM-dd}
- Værelse:   {room}

Vi glæder os til at se dig ✨
— Bobby's Booking
";
    msg.Body = new BodyBuilder { TextBody = bodyText }.ToMessageBody();

    try
    {
        using var client = new SmtpClient();
        await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(smtpUser, smtpPass);
        await client.SendAsync(msg);
        await client.DisconnectAsync(true);
        return Results.Ok(new { ok = true, mail = "sent" });
    }
    catch (Exception ex)
    {
        Console.WriteLine("SMTP fejl: " + ex.Message);
        return Results.Ok(new { ok = true, mail = "failed", error = ex.Message });
    }
});

app.Run();
