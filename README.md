# Seq.App.Pushover

Forwards events to Pushover (https://pushover.net/).

## Installing from nuget:
* In seq, go to 'settings' >> 'APPS' >> 'INSTALL FROM NUGET'.
* Select the 'nuget.org' feed.
* In the 'Package id' box, type: ```Seq.App.Pushover```

Press 'INSTALL' and you're good to go!

## Configuring the App:
* In seq, go to 'settings' >> 'APPS'.
* In 'PUSHOVER APP', Start a new instance.
* Add your Pushover ApiKey.
* Add a DisplayTitle that is going to appear in the notifications. *(Log properties may be used)*
* Add a MessageTemplate to be pushed when triggered. *(Log properties may be used)*
* Optionaly, add a UserKey to identify who is going to receive notifications.
* Optionaly, add one or more Devices *(separeted by pipe and no spaces)* that is going to receive notifications.
* Optionaly, specify a SupressionTime to supress repeated events in a given time in seconds.

SAVE CHANGES and the app will be up and running!

## Using properties in DisplayTitle and MessageTemplate:
* Any property can be applied using the **{{PropName}}** sintax.
  
  Suppose your MessageTemplate is:
    
    ```Hello from {{Country}}!```
    
    If your event contains a property named 'Country' with the value 'Brazil', then the result message will be as follows:
    
    ```Hello from Brazil!```

* The same way, sub-properties can be applied using dots:

  ``` Hello there, I'm {{Person.Name}}, nice to meet you! ``` = ``` Hello there, I'm Matheus, nice to meet you! ```
  
  ``` Billed amount $ {{Request.Payment.Amount}} ``` = ``` Billed amount $ 1.00 ```

* Built-in properties are specified starting with @ in the name.

  ``` {{@Level}} - {{@RenderedMessage}} ``` = ``` Fatal - Application aborted! ```

* Any non-existent property will return null.
  
  ``` Hello there, I'm {{WrongName}}, nice to meet you! ``` = ``` Hello there, I'm , nice to meet you! ```


## Built-in properties:
* @Id - Seq event id.
* @TimestampUtc - Event timestamp in utc.
* @EventType - Seq event type.
* @Exception - Exception content.
* @Level - Log level *(e.g. Information, Warning, Error, etc)*
* @LocalTimestamp - Event timestamp in local time.
* @MessageTemplate - The template used in the event.
* @RenderedMessage - The rendered message.