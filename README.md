# TechShelf

## Introduction
This is an e-commerce application built with ASP.NET Core and Angular for a fictional company called TechShelf, which sells various devices such as phones and laptops. It provides a comprehensive platform for users to browse, search, and purchase products online and for company employees (administrators) to manage product information.

## Features
- User registration and authentication
- Password reset functionality
- Product search (individual products, pagination, filtering)
- Shopping cart management
- Order placement and tracking
- Secure checkout with Stripe

## Technologies
- Backend: ASP.NET Core, Entity Framework Core, Indentity Framework
- Backend libraries: FluentValidation, MediatR, ErrorOr, SendGrid, Stripe.net, Ardalis.Specification
- Backend testring: xUnit, Moq, AutoFixture, Fluent Assertions (will be replaced with Shouldly)
- Frontend: Angular, Angular Material, Tailwind CSS
- Database: PostgreSQL

## The project is still in development. My future plans include:
- Allowing administrators to edit product information and update order statuses
- Refactoring the Angular code and adding test coverage
- Improving UI/UX for a better user experience
- Allowing administrators to specify product specifications (e.g., RAM, processor, etc.)
