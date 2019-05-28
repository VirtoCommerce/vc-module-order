# Scenarios

In order to view and manage Orders in the ‘Orders Module’ the user shall be assigned to the ‘Order’ role with a certain set of permissions.

## Add Role and Permissions

![Fig.Customer Orders](/docs/media/diagram-add-roles-and-permissions.png)

1. The admin navigates to browse->Security->Roles.
1. The system displays the ‘Roles’ screen.
1. The admin clicks the ‘Add’ button to create a new role.
1. The system displays the ‘New Role’ screen that contains the following elements:
     1. Name input field;
     1. Description input field;
     1. List of permissions. Each set of permissions contains:
         1. Permission name;
         1. Sectable list of permissions.
![Fig.Customer Orders](/docs/media/screen-permission-name.png)
![Fig.Customer Orders](/docs/media/screen-assign-permissions.png)

1. The admin fills out the fields, assigns permissions to the new role and clicks the ‘Create’ button.
1. The new role with permissions is created.

**Important**: The admin can select different set of permissions for multiple roles related to Orders Module. Roles can be edited and deleted. Permissions can be edited or removed from the role.

## Assign Order Role to User

![Fig.Customer Orders](/docs/media/diagram-assign-role-with-permissions.png)

1. The admin navigates to Browse->Security->Users and selects a user.
1. The system displays the user details.
1. The admin selects the Roles widget that can be either empty or contains the roles previously assigned to the selected user.
1. The admin clicks the ‘Assign’ button to assign a new role to the user.
1. The admin selects the role(s) and confirms the selection by clicking the ‘OK’ button.
1. The system will assign the selected role to the user.

## Assign Permissions to Order Manager role

1. The admin navigates to Browse->Security->Roles and selects the 'order manager' role
1. The system will display the 'Order manager' blade :

    1. Summary

        1. 'Name';
        1. 'Description'.
    1. Assigned Permissions (list with check boxes):

        1. order:access (open orders menu);
        1. order:create (create order);
        1. order:delete (delete order);
        1. order:read (view order data);
        1. order:update (update order data).

1. The admin enters the order manager name, description and selects the permission(s) that will be assigned to the role.
1. The assigned permissions will be applied to the 'order manager' role.

![Fig.Customer Orders](/docs/media/screen-assign-permissions-to-order-manager.png)

## Assign bounded scope to 'order manager' role

1. The admin selects the permission(s) assigned to the 'order manager' role.
1. The system will open the next blade - 'View order data' and prompt the user to configure permissions scope.
![Fig.Customer Orders](/docs/media/screen-select-bounded-scope.png)

1. The admin selects one of the following options or both:

    1. Only for orders in selected stores;
    1. Only for order responsible.

1. If 'Only for orders in selects stores' option is selected, the system will open the next blade and prompt the admin to choose the store(s).
![Fig.Customer Orders](/docs/media/screen-select-stores.png)
1. The admin selects the store(s) and confirms selection by clicking the 'OK' button.
1. The assigned permissions and bounded scope will be applied to the 'order manager' role.
