# OneCog.Io.Owl.Intuition.Device.State

Provides a state machine driven interface for setting up and receiving readings from an Owl Inution device.

## OneCog.Io.Owl.Intuition.Device.State.Machine

Represents a state machine instance which follows the following transitions:


> Disconnected -> Connecting -> Configuring -> Listening -> Disconnected
>                            -> Faulted     -> Faulted