-- Seed Roles
INSERT INTO [dbo].[AspNetRoles] ([Id],[Name],[NormalizedName],[ConcurrencyStamp])
VALUES 
(1, 'Admin', 'ADMIN', NEWID()),
(2, 'RegistryOfficer', 'REGISTRYOFFICER', NEWID()),
(3, 'Drafter', 'DRAFTER', NEWID()),
(4, 'SeniorPC', 'SENIORPC', NEWID()),
(5, 'MDAOfficer', 'MDAOFFICER', NEWID());

-- Seed Users (password hashes for Admin@123, etc.)
INSERT INTO [dbo].[AspNetUsers]
([Id],[UserName],[NormalizedUserName],[Email],[NormalizedEmail],[EmailConfirmed],[PasswordHash],[SecurityStamp],[ConcurrencyStamp],[AccessFailedCount])
VALUES
(1, 'admin@opc.ls', 'ADMIN@OPC.LS', 'admin@opc.ls', 'ADMIN@OPC.LS', 1, 'AQAAAAIAAYagAAAAEHlnkHg5qp7lXcEk/YFUsTHQkE+2KhPgSTUGJzFyScVdj2h0DhhTDIMJQl8m1H1xVQ==', NEWID(), NEWID(), 0),
(2, 'registry@opc.ls', 'REGISTRY@OPC.LS', 'registry@opc.ls', 'REGISTRY@OPC.LS', 1, 'AQAAAAIAAYagAAAAEBA5uS5MT3O2Zwh02Z2GDrWQBOa6olY6quhFVCmR3OYYoU61YvETv7kpZ1ksuWrNDA==', NEWID(), NEWID(), 0),
(3, 'drafter@opc.ls', 'DRAFTER@OPC.LS', 'drafter@opc.ls', 'DRAFTER@OPC.LS', 1, 'AQAAAAIAAYagAAAAEIz1Y/HlHsY+ecSKpqqXNoNUTxdFv1Z1zZQgbJRxJxBqz9xQ8bYaJbDJrVY72N9XZQ==', NEWID(), NEWID(), 0),
(4, 'senior@opc.ls', 'SENIOR@OPC.LS', 'senior@opc.ls', 'SENIOR@OPC.LS', 1, 'AQAAAAIAAYagAAAAEGy7Z1wPkvEVGViN9Wk13FwM66jN0Ovj+ldAjvutnDBtTiK/AOzGYOVGQhrwz+RQzQ==', NEWID(), NEWID(), 0),
(5, 'mda@ministry.ls', 'MDA@MINISTRY.LS', 'mda@ministry.ls', 'MDA@MINISTRY.LS', 1, 'AQAAAAIAAYagAAAAEDjGgKxqTnkk+k5C+jDsN+0HVtF8ZgxjZSmTnWb7YWhMejF4L4msfEwjYb/fg1R6Gw==', NEWID(), NEWID(), 0);

-- Link Users to Roles
INSERT INTO [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES
(1, 1), (2, 2), (3, 3), (4, 4), (5, 5);