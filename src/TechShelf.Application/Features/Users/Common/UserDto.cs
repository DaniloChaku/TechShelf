﻿namespace TechShelf.Application.Features.Users.Common;

public record UserDto(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    IEnumerable<string> Roles);
