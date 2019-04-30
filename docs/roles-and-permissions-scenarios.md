# Scenarios

In order to view and manage Orders in the ‘Orders Module’ the user shall be assigned to the ‘Order’ role with a certain set of permissions.

## Add Role and Permissions

![Fig.Customer Orders](/docs/media/diagram-add-roles-and-permissions.png)

1. The admin navigates to browse->Security->Roles 
1. The system displays the ‘Roles’ screen
1. The admin clicks the ‘Add’ button to create a new role
1. The system displays the ‘New Role’ screen that contains the following elements: 
     1. Name input field
     1. Description input field 
     1. List of permissions. Each set of permissions contains:
         1. Permission name
         1. Sectable list of permissions
![Fig.Customer Orders](/docs/media/screen-permission-name.png)
![Fig.Customer Orders](/docs/media/screen-assign-permissions.png)

1. The admin fills out the fields, assigns permissions to the new role and clicks the ‘Create’ button
1. The new role with permissions is created
Important: The admin can select different set of permissions for multiple roles related to Orders Module. Roles and permissions can be both edited and deleted from the system.

## Assign Order Role to User

![Fig.Customer Orders](/docs/media/diagram-assign-role-with-permissions.png)

1. The admin navigates to Browse->Security->Users and selects a user
1. The system displays the user details
1. The admin selects the Roles widget that can be either empty or contains the roles previously assigned to the selected user
1. The admin clicks the ‘Assign’ button to assign a new role to the user
1. The admin selects the role(s) and confirms the selection by clicking the ‘OK’ button 
1. The system will assign the selected role to the user
