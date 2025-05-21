Feature: Todo propagation
    When a todo is created from elsewhere, we receive it

    Scenario: Receiving a todo
        Given I am on the homepage
        And another browser opens on the homepage
        When adding a todo
        Then the todo is displayed on the other browser

    Scenario: See all todos on page open
        Given I am on the homepage
        When adding a todo
        And waiting for the todo to be saved
        And another browser opens on the homepage
        Then the todo is displayed on the other browser