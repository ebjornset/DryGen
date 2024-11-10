Feature: .Net shared folders
To be able to use dry-gen with assemblies located in the .Net shared folders
As a dry-gen user
I should get the Asp.Net Core shared folder name resolved by the .Net shared folder

  Scenario: Resolve Asp.Net Core shared folders from .Net version
    Given the base .Net shared folder is 'Microsoft.NETCore.App'
    Given these .Net shared folder paths
      | Base folder              | Sub folder               |
      | Microsoft.NETCore.App    | <.Net version>           |
      | Microsoft.AspNetCore.App | <Asp.Net Core version 1> |
      | Microsoft.AspNetCore.App | <Asp.Net Core version 2> |
    When I resolve the Asp.Net Core shared folder with .Net version '<.Net version>'
    Then I should get the folder Asp.Net Core shared folder 'Microsoft.AspNetCore.App/<Asp.Net Core version found>'

    Examples:
      | .Net version        | Asp.Net Core version 1 | Asp.Net Core version 2 | Asp.Net Core version found |
      |              8.0.10 |                 6.0.30 |                 8.0.10 |                     8.0.10 |
      |  9.0.0-rc.2.24473.5 |                 8.0.10 |     9.0.0-rc.2.24474.3 |         9.0.0-rc.2.24474.3 |
      | 9.0.0-rc.2.24473.5/ |                 8.0.10 |     9.0.0-rc.2.24474.3 |         9.0.0-rc.2.24474.3 |
