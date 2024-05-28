using ContactList.Core.Dtos;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.IntegrationTests
{
    public class ContactsControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ContactsControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAllContactsForUser_ReturnsOkWithContacts()
        {
            // Arrange (przygotowanie)
            var token = await GetJwtToken("artur@goldlab.pl", "as1as1A!"); // Pobierz token JWT dla użytkownika
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act (akcja)
            var response = await _client.GetAsync("/api/v1/contacts/GetAllContacts");

            // Assert (asercje)
            response.EnsureSuccessStatusCode();
            var contacts = await response.Content.ReadFromJsonAsync<IEnumerable<ContactDto>>();
            Assert.NotNull(contacts);
            Assert.NotEmpty(contacts);
        }

        [Fact]
        public async Task CreateContact_ValidRequest_ReturnsCreatedAtAction()
        {
            // Arrange
            var token = await GetJwtToken("jan.kowalski@example.com", "User123!");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var newContact = new CreateContactRequestDto
            {
                FirstName = "Nowy",
                LastName = "Kontakt",
                Email = "nowy.kontakt@example.com",
                PhoneNumber = "987654321",
                CategoryId = 1,
                SubcategoryId = 1
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/contacts", newContact);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var createdContact = await response.Content.ReadFromJsonAsync<ContactDto>();
            Assert.NotNull(createdContact);
            Assert.Equal(newContact.FirstName, createdContact.FirstName);
        }

        // ... (inne testy integracyjne dla ContactsController)

        private async Task<string> GetJwtToken(string email, string password)
        {
            // Przygotowanie danych logowania
            var loginDto = new LoginRequestDto { Email = email, Password = password };

            // Wywołanie endpointu logowania (używamy HttpClient z klasy WebApplicationFactory)
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);

            // Sprawdzenie, czy logowanie się powiodło
            response.EnsureSuccessStatusCode();

            // Odczytanie tokenu z odpowiedzi
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            return loginResponse?.Token; // Zwrócenie tokenu lub null, jeśli nie został znaleziony
        }

        // Klasa pomocnicza do deserializacji odpowiedzi logowania
        public class LoginResponseDto
        {
            public string Token { get; set; }
        }
    }
}
