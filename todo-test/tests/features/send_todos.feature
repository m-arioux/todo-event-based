Feature: Send todos
    We can send todos

    Scenario: Adding a todo
        Given I am on the homepage
        When adding a todo
        Then the todo is displayed


    Scenario: Clearing todo field
        Given I am on the homepage
        When adding a todo
        Then the field should clear

